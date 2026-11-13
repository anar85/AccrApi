namespace AccrServicesApi.Models.ViewModels
{
	public class CreditDetailDto
	{
		public long CreditId { get; set; }
		public long BatchId { get; set; }
		public int Status { get; set; }
		public string StatusName { get; set; } = string.Empty;
		public string? NumDog { get; set; }
		public string? AccountNo { get; set; }

		/// <summary>
		/// CBAR kredit qrafiki: borrower, collaterals, guarantees və bütün nested sahələr.
		/// </summary>
		public Credit Credit { get; set; } = null!;

		public List<CreditApiErrorDto> Errors { get; set; } = new();
	}
}
