using AccrServicesApi.Atributes;
using AccrServicesApi.Data.Logs;
using AccrServicesApi.Models.Settings;
using System.Text;

namespace AccrServicesApi.Middleware
{
    public class RequestResponseLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly ISaveLogs _saveLogs;

        public RequestResponseLoggingMiddleware(ILogger<RequestResponseLoggingMiddleware> logger, ISaveLogs saveLogs)
        {
            _logger = logger;
            _saveLogs = saveLogs;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var endpoint = context.GetEndpoint();

            var hasLogging = endpoint?.Metadata.GetMetadata<RequestResponseLoggingAttribute>() != null;

            if (!hasLogging)
            {
                await next(context);
                return;
            }

            var logGuid = Guid.NewGuid().ToString();
            context.Items["LogGuid"] = logGuid;
            await LogRequest(context, logGuid);

            var originalResponseBody = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                try
                {
                    await next(context);
                    await LogResponse(context, responseBody, originalResponseBody, logGuid);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An exception occurred: {ex.Message}");
                    //throw new BadRequestException(ExceptionCodes.UnexpectedError, ex.Message);
                }
                finally
                {
                    context.Response.Body = originalResponseBody;
                }
            }
        }

        private async Task LogRequest(HttpContext context, string logGuid)
        {
            context.Request.EnableBuffering();

            string content;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                content = await reader.ReadToEndAsync();
            }

            context.Request.Body.Position = 0;
            var clientIp = GetClientIpFromHeaders(context); //context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var requestLog = new LogEntry
            {
                LogGuid = logGuid,
                HttpMethod = context.Request.Method,
                Url = context.Request.Path,
                Headers = string.Join("; ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")),
                Body = content,
                Type = "Request",
                IpAdress = clientIp
            };

            await _saveLogs.SaveLogAsync(requestLog);

            _logger.LogInformation(content);
        }

        private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalResponseBody, string logGuid)
        {
            responseBody.Seek(0, SeekOrigin.Begin);

            string content;
            using (var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true))
            {
                content = await reader.ReadToEndAsync();
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await using (var writer = new MemoryStream())
            {
                await responseBody.CopyToAsync(writer);
                writer.Seek(0, SeekOrigin.Begin);
                await writer.CopyToAsync(originalResponseBody);
            }

            context.Response.Body = originalResponseBody;

            var responseLog = new LogEntry
            {
                LogGuid = logGuid,
                HttpMethod = context.Request.Method,
                Url = context.Request.Path,
                Headers = string.Join("; ", context.Response.Headers.Select(h => $"{h.Key}: {h.Value}")),
                Body = content,
                Type = "Response",
                StatusCode = context.Response.StatusCode
            };

            await _saveLogs.SaveLogAsync(responseLog);

            _logger.LogInformation(content);
        }

        private string GetClientIpFromHeaders(HttpContext context)
        {
            var headersToCheck = new[] { "X-Forwarded-For", "X-Real-IP" };

            foreach (var header in headersToCheck)
            {
                if (context.Request.Headers.TryGetValue(header, out var headerValue))
                {
                    var clientIp = headerValue.ToString().Split(',').FirstOrDefault();
                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        return clientIp.Trim();
                    }
                }
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
