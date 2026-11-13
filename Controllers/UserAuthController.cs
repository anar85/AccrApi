using AccrServicesApi.Models.ViewModels;
using AccrServicesApi.Services.ViewService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccrServicesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
		private readonly IUiJobsService _uiJobsService;

		public UserAuthController(IUiJobsService uiJobsService)
		{
			_uiJobsService = uiJobsService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
		{
			if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
			{
				return BadRequest(new { Message = "İstifadəçi adı və ya şifrə boş ola bilməz." });
			}

			var response = await _uiJobsService.LoginAsync(request);

			if (response.IsSuccess)
			{
				return Ok(new
				{
					Message = "Giriş uğurludur.",
					//User = response.OraUser,
					ErrorCode = response.ErrorCode,
					//LogMessage = response.ErrorMessage 
				});
			}

			return Unauthorized(new
			{
				Message = "İstifadəçi adı və ya şifrə yanlışdır.",
				ErrorCode = response.ErrorCode,
				ErrorDetail = response.ErrorMessage
			});
		}
	}
}
