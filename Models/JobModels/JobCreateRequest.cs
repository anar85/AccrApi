using System.ComponentModel.DataAnnotations;

namespace AccrServicesApi.Models.JobModels
{
	public class JobCreateRequest
	{
		[Required]
		public DateOnly DateFrom { get; set; }

		[Required]
		public DateOnly DateTo { get; set; }
		[Required]
		public int ReportType { get; set; }
		public List<string> ContractList { get; set; }
	}
}
