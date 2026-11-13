namespace AccrServicesApi.Models.ViewModels
{
	public class CreditApiErrorDto
	{
		public long ErrorId { get; set; }
		public long BatchId { get; set; }
		public long CreditId { get; set; }
		public string NumDog { get; set; }
		public string ErrorType { get; set; }
		public string DocumentNo { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
