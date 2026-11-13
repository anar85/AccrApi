using AccrServicesApi.Models.AccrServiceModels;
using AccrServicesApi.Models.Settings;
using AccrServicesApi.Services.JobService;
using AccrServicesApi.Services.JwtService;
using Microsoft.AspNetCore.Http;
using AccrServicesApi.Data.Logs;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AccrServicesApi.Services.ApiClient
{
    public class ApiClientService : IApiClientService
    {
        private readonly ILogger<ApiClientService> _logger;
        private readonly CbarApiSettings _settings;
        private readonly IJobService _service;
        private readonly HttpClient _httpClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISaveLogs _saveLogs;

        public ApiClientService(ILogger<ApiClientService> logger, IOptions<CbarApiSettings> options, IJobService service, HttpClient httpClient, IJwtGenerator jwtGenerator, IHttpContextAccessor httpContextAccessor, ISaveLogs saveLogs)
        {
            _logger = logger;
            _settings = options.Value;
            _service = service;
            _httpClient = httpClient;
            _jwtGenerator = jwtGenerator;
            _httpContextAccessor = httpContextAccessor;
            _saveLogs = saveLogs;
        }

        public async Task<ApiSendResult> SendToApiAsync(long batchId, string jsonPayload)
        {
            try
            {
                _logger.LogInformation("Batch {BatchId} üçün JSON hazırdır. Ölçü: {Length} simvol. Token generasiya olunur...", batchId, jsonPayload.Length);

                var jwt = _jwtGenerator.CreateJwtRs256(
                    issuer: _settings.JwtIssuer,
                    audienceDomain: _settings.JwtAudienceDomain,
                    lifetime: TimeSpan.FromMinutes(_settings.JwtLifetimeMinutes)
                );

                using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                request.Headers.Add("API-KEY", _settings.ApiKey);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var logGuid = _httpContextAccessor?.HttpContext?.Items["LogGuid"] as string ?? Guid.NewGuid().ToString();

                // Save outgoing request
                try
                {
                    var reqHeaders = string.Join("; ", request.Headers.Select(h => $"{h.Key}: {string.Join(',', h.Value)}"));
                    if (request.Content?.Headers != null)
                        reqHeaders += "; " + string.Join("; ", request.Content.Headers.Select(h => $"{h.Key}: {string.Join(',', h.Value)}"));

                    var outgoingRequestLog = new LogEntry
                    {
                        LogGuid = logGuid,
                        HttpMethod = HttpMethod.Post.Method,
                        Url = _settings.BaseUrl,
                        Headers = reqHeaders,
                        Body = jsonPayload,
                        Type = "ExternalRequest",
                        BatchId = batchId
                    };

                    await _saveLogs.SaveLogAsync(outgoingRequestLog);
                }
                catch
                {

                }

                _logger.LogInformation("API-yə POST sorğusu göndərilir: {BaseUrl}", _settings.BaseUrl);
                _logger.LogInformation("Request to api: {jsonPayload}", jsonPayload);

                using var response = await _httpClient.SendAsync(request);

                string responseBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                // Save outgoing response
                try
                {
                    var respHeaders = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {string.Join(',', h.Value)}"));
                    if (response.Content?.Headers != null)
                        respHeaders += "; " + string.Join("; ", response.Content.Headers.Select(h => $"{h.Key}: {string.Join(',', h.Value)}"));

                    var outgoingResponseLog = new LogEntry
                    {
                        LogGuid = (_httpContextAccessor?.HttpContext?.Items["LogGuid"] as string) ?? Guid.NewGuid().ToString(),
                        HttpMethod = HttpMethod.Post.Method,
                        Url = _settings.BaseUrl,
                        Headers = respHeaders,
                        Body = responseBody,
                        Type = "ExternalResponse",
                        StatusCode = statusCode,
                        BatchId = batchId
                    };

                    await _saveLogs.SaveLogAsync(outgoingResponseLog);
                }
                catch
                {
                    // ignore logging failures
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Response from api: {responseBody}", JsonSerializer.Serialize(responseBody, options));


                    //var cbarResponse = JsonSerializer.Deserialize<CbarResponseDto>(responseBody,
                    //    new JsonSerializerOptions
                    //    {
                    //        PropertyNameCaseInsensitive = true
                    //    }
                    //);

                    if (!TryDeserializeJson(responseBody, out CbarResponseDto? cbarResponse))
                    {
                        var rejectedMessage = BuildNonJsonResponseMessage(statusCode, responseBody);
                        _logger.LogError(
                            "Batch {BatchId} üçün API JSON əvəzinə rədd cavabı qaytardı. {Message}",
                            batchId,
                            rejectedMessage);

                        await _service.UpdateBatchRequestIdAsync(batchId, null!, 3);

                        return new ApiSendResult
                        {
                            IsSent = false,
                            Message = rejectedMessage,
                            StatusCode = statusCode,
                            ApiResponse = responseBody
                        };
                    }

                    _logger.LogInformation("Batch {BatchId} uğurla göndərildi. RequestId: {RequestId}", batchId, cbarResponse?.RequestId);

                    await _service.UpdateBatchRequestIdAsync(batchId, cbarResponse?.RequestId ?? string.Empty, 2);

                    return new ApiSendResult
                    {
                        IsSent = true,
                        Message = "Paket uğurla göndərildi.",
                        StatusCode = statusCode,
                        RequestId = cbarResponse?.RequestId,
                        ApiResponse = responseBody
                    };
                }

                _logger.LogError("API Xətası! BatchId: {BatchId}, Status Code: {StatusCode}. Detal: {ResponseBody}", batchId, statusCode, responseBody );

                await _service.UpdateBatchRequestIdAsync(batchId, null!, 3);

                return new ApiSendResult
                {
                    IsSent = false,
                    Message = $"CBAR API xətası (HTTP {statusCode}) + {responseBody}",
                    StatusCode = statusCode,
                    ApiResponse = responseBody
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch {BatchId} göndərilərkən sistem xətası baş verdi.", batchId);

                await _service.UpdateBatchRequestIdAsync(batchId, null!, 3);

                return new ApiSendResult
                {
                    IsSent = false,
                    Message = ex.Message,
                    ApiResponse = ex.ToString()
                };
            }
        }

        public async Task<CheckStatusResponseDto> CheckBatchStatusAsync(string requestId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                _logger.LogError("CheckBatchStatusAsync: RequestId boş ola bilməz.");

                return new CheckStatusResponseDto
                {
                    RequestId = requestId,
                    Status = "error",
                    Message = "RequestId boşdur."
                };
            }

            try
            {
                _logger.LogInformation(
                    "RequestId {RequestId} üçün status sorğusu hazırlanır. Token generasiya olunur...",
                    requestId
                );

                var jwt = _jwtGenerator.CreateJwtRs256(
                    issuer: _settings.JwtIssuer,
                    audienceDomain: _settings.JwtAudienceDomain,
                    lifetime: TimeSpan.FromMinutes(_settings.JwtLifetimeMinutes)
                );

                string statusUrl = $"{_settings.BaseUrl.TrimEnd('/')}/{requestId}/status";

                using var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                request.Headers.Add("API-KEY", _settings.ApiKey);

                _logger.LogInformation("API-yə GET sorğusu göndərilir: {StatusUrl}", statusUrl);

                using var response = await _httpClient.SendAsync(request, ct);

                string responseBody = await response.Content.ReadAsStringAsync(ct);
                int statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    //var result = JsonSerializer.Deserialize<CheckStatusResponseDto>(
                    //    responseBody,
                    //    new JsonSerializerOptions
                    //    {
                    //        PropertyNameCaseInsensitive = true
                    //    }
                    //);

                    if (!TryDeserializeJson(responseBody, out CheckStatusResponseDto? result))
                    {
                        var rejectedMessage = BuildNonJsonResponseMessage(statusCode, responseBody);
                        _logger.LogError(
                            "RequestId {RequestId} üçün status cavabı JSON deyil. {Message}",
                            requestId,
                            rejectedMessage);

                        return new CheckStatusResponseDto
                        {
                            RequestId = requestId,
                            Status = "rejected",
                            Message = rejectedMessage
                        };
                    }

                    _logger.LogInformation(
                        "RequestId {RequestId} üzrə status uğurla oxundu. Cari Status: {Status}",
                        requestId,
                        result?.Status
                    );

                    return result ?? new CheckStatusResponseDto
                    {
                        RequestId = requestId,
                        Status = "error",
                        Message = "API cavabı boş və ya deserialize edilə bilmədi.",
                    };
                }

                if (statusCode == 404)
                {
                    _logger.LogWarning(
                        "RequestId {RequestId} sistemdə tapılmadı HTTP 404. Detal: {ResponseBody}",
                        requestId,
                        responseBody
                    );

                    return new CheckStatusResponseDto
                    {
                        RequestId = requestId,
                        Status = "not_found",
                        Message = "Axtarış edilən sorğu tapılmadı."
                    };
                }

                _logger.LogError(
                    "CBAR Status API Xətası! RequestId: {RequestId}, Status Code: {StatusCode}. Detal: {ResponseBody}",
                    requestId,
                    statusCode,
                    responseBody
                );

                return new CheckStatusResponseDto
                {
                    RequestId = requestId,
                    Status = "api_error",
                    Message = $"API xətası (HTTP {statusCode}). Detallar: {responseBody}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RequestId {RequestId} üçün status sorğulanarkən sistem xətası baş verdi.",
                    requestId
                );

                return new CheckStatusResponseDto
                {
                    RequestId = requestId,
                    Status = "system_error",
                    Message = $"Daxili sistem xətası: {ex.Message}"
                };
            }
        }

        private static bool TryDeserializeJson<T>(string responseBody, out T? value) where T : class
        {
            value = null;
            if (string.IsNullOrWhiteSpace(responseBody))
                return false;

            var trimmed = responseBody.TrimStart();
            if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
                return false;

            try
            {
                value = JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string BuildNonJsonResponseMessage(int statusCode, string responseBody)
        {
            var supportId = ExtractSupportId(responseBody);
            if (!string.IsNullOrEmpty(supportId))
            {
                return $"Sorğu rədd edildi (HTTP {statusCode}). Support ID: {supportId}. Administratorla əlaqə saxlayın.";
            }

            return $"API JSON əvəzinə HTML və ya mətn cavab qaytardı (HTTP {statusCode}).";
        }

        private static string? ExtractSupportId(string responseBody)
        {
            const string marker = "support ID is:";
            var index = responseBody.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            var rest = responseBody[(index + marker.Length)..].TrimStart();
            var end = 0;
            while (end < rest.Length && char.IsDigit(rest[end]))
                end++;

            return end == 0 ? null : rest[..end];
        }
    }
}