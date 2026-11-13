namespace AccrServicesApi.Models.JobModels
{
	public class JobCreateResponse
	{
		public long JobId { get; set; }
		public int Status { get; set; } // 0=PENDING
	}
}
