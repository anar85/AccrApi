namespace AccrServicesApi.Models.AccrServiceModels
{
	public class ApiSendResult
	{
		public bool IsSent { get; set; }
		public string Message { get; set; } = string.Empty;
		public int? StatusCode { get; set; }
		public string? RequestId { get; set; }
		public string? ApiResponse { get; set; }
	}
}
