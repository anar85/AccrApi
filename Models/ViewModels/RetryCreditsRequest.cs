using System.ComponentModel.DataAnnotations;

namespace AccrServicesApi.Models.ViewModels
{
	public class RetryCreditsRequest
	{
		[Required]
		public List<long> CreditIds { get; set; } = new();
	}
}
