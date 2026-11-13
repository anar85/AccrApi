namespace AccrServicesApi.Models.ViewModels
{
	public class CreditListItemDto
	{
		public long CreditId { get; set; }
		public long BatchId { get; set; }
		public string? NumDog { get; set; }
		public string? AccountNo { get; set; }
		public int Status { get; set; }
		public string StatusName { get; set; }
		public string? ErrorMessage { get; set; }
		public int RetryCount { get; set; }
		public DateTime? LastRetryAt { get; set; }
	}
}
