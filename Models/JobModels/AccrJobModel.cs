namespace AccrServicesApi.Models.JobModels
{
	public class AccrJobModel
	{
		public long Id { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public int Status { get; set; }
		public int ReportType { get; set; }
	}
}
