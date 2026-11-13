namespace AccrServicesApi.Models.JobModels
{
	public sealed class JobInfo
	{
		public long JobId { get; init; }
		public DateTime DateFrom { get; init; }
		public DateTime DateTo { get; init; }
		public string? LastAccountNo { get; init; }
		public int Status { get; init; }
	}
}
