using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AccrServicesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(ILogger<TestController> logger, IWebHostEnvironment env) : ControllerBase
{
    private readonly ILogger<TestController> _logger = logger;
	private readonly IWebHostEnvironment _env = env;

	[HttpPost("armb")]
	public async Task<IActionResult> CallArmb([FromBody] AccrRequest request)
	{
		var privateKeyPath = Path.Combine(_env.ContentRootPath, "PrivateKey", "accr_prod_private.pem");

		var jwt = JwtGenerator.CreateJwtRs256(
			pkcs8PemPath: privateKeyPath,
			issuer: "1136",
			audienceDomain: "gw-accr.e-cbar.az",
			lifetime: TimeSpan.FromMinutes(15)
		);

		var apiKey = "pj64oaOqGypgnB7fnV020OAnArixjViA";
		var url = "https://gw-accr.e-cbar.az/public/api/v1/loans";

		using var http = new HttpClient();
		http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
		http.DefaultRequestHeaders.Add("API-KEY", apiKey);

		var resp = await http.PostAsJsonAsync(url, request);
		var body = await resp.Content.ReadAsStringAsync();

		return StatusCode((int)resp.StatusCode, new
		{
			status = (int)resp.StatusCode,
			response = body,
			tokenSample = jwt
		});
	}
}
