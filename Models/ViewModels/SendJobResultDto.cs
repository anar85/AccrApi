namespace AccrServicesApi.Models.ViewModels
{
	public class SendJobResultDto
	{
		public long JobId { get; set; }
		public int SentCount { get; set; }
		public int FailedCount { get; set; }
		public bool IsCompleted { get; set; }
		public string Message { get; set; } = string.Empty;
		public List<SendBatchResultDto> BatchResults { get; set; } = new();
	}
}
