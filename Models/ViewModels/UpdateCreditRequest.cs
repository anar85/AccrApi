namespace AccrServicesApi.Models.ViewModels
{
	public class UpdateCreditRequest
	{
		/// <summary>
		/// Redaktə olunmuş tam kredit qrafiki (borrower, collaterals, guarantees daxil).
		/// </summary>
		public Credit Credit { get; set; } = null!;

		/// <summary>
		/// true olanda kredit retry növbəsinə düşür (yenidən göndərilə bilər).
		/// </summary>
		public bool MarkForRetry { get; set; } = true;
	}
}
