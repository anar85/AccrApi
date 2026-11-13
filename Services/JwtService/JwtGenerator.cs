using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AccrServicesApi
{
	public static class JwtGenerator
	{
		public static string CreateJwtRs256(string pkcs8PemPath, string issuer, string audienceDomain, TimeSpan lifetime)
		{
			if (lifetime > TimeSpan.FromMinutes(15))
				throw new ArgumentException("Token lifetime maksimum 15 dəqiqə olmalıdır.");


			var nowUtc = DateTimeOffset.UtcNow;
			var nbfUtc = nowUtc.AddSeconds(-150);
			var expiresUtc = nowUtc.Add(lifetime);

			// RSA private key import (PKCS#8 PEM)
			using var rsa = RSA.Create();
			var pem = File.ReadAllText(pkcs8PemPath);
			rsa.ImportFromPem(pem);

			var key = new RsaSecurityKey(rsa);


			var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

			var iatUnix = nowUtc.ToUnixTimeSeconds();

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Iat, iatUnix.ToString(), ClaimValueTypes.Integer64),

			};

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audienceDomain,
				claims: claims,
				notBefore: nbfUtc.UtcDateTime,
				expires: expiresUtc.UtcDateTime,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
