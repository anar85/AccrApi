namespace AccrServicesApi.Models.ViewModels
{
	public class UpdateCreditResponseDto
	{
		public long CreditId { get; set; }
		public long BatchId { get; set; }
		public bool Updated { get; set; }
		public bool MarkedForRetry { get; set; }
		public string Message { get; set; } = string.Empty;
	}
}
