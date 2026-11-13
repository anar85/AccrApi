namespace AccrServicesApi.Models.Settings
{
	public class CbarApiSettings
	{
		public string BaseUrl { get; set; }
		public string ApiKey { get; set; }
		public string BankName { get; set; }
		public string BankID { get; set; }
		public string ResponsiblePersonName { get; set; }
		public string ResponsiblePersonJobTitle { get; set; }
		public string ResponsiblePersonPinCode { get; set; }
		public string ResponsiblePersonId { get; set; }
		public string ResponsiblePersonDob { get; set; }
		public string PrivateKeyFileName { get; set; }
		public string JwtIssuer { get; set; }
		public string JwtAudienceDomain { get; set; }
		public int JwtLifetimeMinutes { get; set; }
	}
}
