namespace AccrServicesApi.Models.ViewModels
{
	public class RetryResultDto
	{
		public int RequestedCount { get; set; }
		public int AcceptedCount { get; set; }
		public int SkippedCount { get; set; }
		public int FailedCount { get; set; }
		public string Message { get; set; } = string.Empty;
	}
}
