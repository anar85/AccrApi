namespace AccrServicesApi.Services.JwtService
{
	public interface IJwtGenerator
	{
		string CreateJwtRs256(string issuer, string audienceDomain, TimeSpan lifetime);
	}
}
