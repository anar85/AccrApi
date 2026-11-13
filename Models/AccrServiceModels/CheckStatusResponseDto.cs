using System.Text.Json.Serialization;

namespace AccrServicesApi.Models.AccrServiceModels
{
	public class CheckStatusResponseDto
	{
		[JsonPropertyName("requestId")]
		public string RequestId { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; } // "queued", "processing", "completed", "partially_completed", "failed", "not_found"

		[JsonPropertyName("processedCount")]
		public int? ProcessedCount { get; set; }

		[JsonPropertyName("failedCount")]
		public int? FailedCount { get; set; }

		[JsonPropertyName("endProcessDate")]
		public string EndProcessDate { get; set; }

		[JsonPropertyName("timestamp")]
		public string Timestamp { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }

		[JsonPropertyName("errors")]
		public List<CheckStatusErrorDto> Errors { get; set; }
	}
	public class CheckStatusErrorDto
	{
		[JsonPropertyName("accountNo")]
		public string AccountNo { get; set; }

		[JsonPropertyName("type")]
		public string Type { get; set; } // "Borrower" və ya "Guarantor"

		[JsonPropertyName("documentNo")]
		public string DocumentNo { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; }
	}
}
