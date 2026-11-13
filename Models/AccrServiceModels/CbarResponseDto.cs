using System.Text.Json.Serialization;

namespace AccrServicesApi.Models.AccrServiceModels
{
	public class CbarResponseDto
	{
		[JsonPropertyName("requestId")]
		public string RequestId { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }
	}
}
