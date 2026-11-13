namespace AccrServicesApi.Models.ViewModels
{
	public class SendBatchResultDto
	{
		public long BatchId { get; set; }
		public bool IsSent { get; set; }
		public string Message { get; set; } = string.Empty;
		public int? StatusCode { get; set; }
		public string? RequestId { get; set; }
		public string? ApiResponse { get; set; }
	}
}
