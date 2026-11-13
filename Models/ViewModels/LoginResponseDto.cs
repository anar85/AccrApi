namespace AccrServicesApi.Models.ViewModels
{
	public class LoginResponseDto
	{
		public bool IsSuccess { get; set; }
		public string OraUser { get; set; }
		public string OraPass { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
