namespace AccrServicesApi.Models.ViewModels
{
	public class BatchListItemDto
	{
		public long BatchId { get; set; }
		public long JobId { get; set; }
		public DateTime CreatedAt { get; set; }
		public int Status { get; set; }
		public string StatusName { get; set; }
		public string RequestId { get; set; }
		public int TotalCreditCount { get; set; }
		public int FailedCreditCount { get; set; }
        public string CallbackStatus { get; set; }
        public DateTime? CallbackDate { get; set; }
    }
}
