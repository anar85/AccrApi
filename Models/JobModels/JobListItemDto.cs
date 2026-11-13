namespace AccrServicesApi.Models.JobModels
{
	public class JobListItemDto
	{
		public long JobId { get; set; }
		public DateTime? ReportDateFrom { get; set; }
		public DateTime? ReportDateTo { get; set; }
		public int ReportType { get; set; }
		public string ReportTypeName { get; set; } // YENİ
		public int Status { get; set; }
		public string StatusName { get; set; }     // YENİ
		public int? TotalCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string LastError { get; set; }
	}
}
