using AccrServicesApi.Models.AccrServiceModels;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AccrServicesApi.Services.ApiClient
{
    public static partial class CbarValidationErrorParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static Dictionary<int, List<CbarFieldError>> Parse(string? apiResponse)
        {
            var result = new Dictionary<int, List<CbarFieldError>>();
            if (string.IsNullOrWhiteSpace(apiResponse))
                return result;

            var json = ExtractJson(apiResponse);
            if (string.IsNullOrWhiteSpace(json))
                return result;

            CbarValidationErrorResponse? validationResponse;
            try
            {
                validationResponse = JsonSerializer.Deserialize<CbarValidationErrorResponse>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return result;
            }

            if (validationResponse?.Errors == null || validationResponse.Errors.Count == 0)
                return result;

            foreach (var (fieldPath, messages) in validationResponse.Errors)
            {
                if (messages == null || messages.Length == 0)
                    continue;

                var match = CreditIndexRegex().Match(fieldPath);
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out var creditIndex))
                    continue;

                if (!result.TryGetValue(creditIndex, out var fieldErrors))
                {
                    fieldErrors = new List<CbarFieldError>();
                    result[creditIndex] = fieldErrors;
                }

                fieldErrors.Add(new CbarFieldError(fieldPath, string.Join("; ", messages)));
            }

            return result;
        }

        private static string? ExtractJson(string apiResponse)
        {
            var trimmed = apiResponse.Trim();
            if (trimmed.StartsWith('{'))
                return trimmed;

            var jsonStart = trimmed.IndexOf('{');
            return jsonStart >= 0 ? trimmed[jsonStart..] : null;
        }

        [GeneratedRegex(@"Credits\[(\d+)\]", RegexOptions.CultureInvariant)]
        private static partial Regex CreditIndexRegex();
    }

    public sealed record CbarFieldError(string FieldPath, string Message);
}
