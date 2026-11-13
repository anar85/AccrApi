using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AccrServicesApi.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AccrServicesApi.Services.JwtService
{
	public sealed class JwtGeneratorService : IJwtGenerator, IDisposable
	{
		private readonly RSA _rsa;
		private readonly ILogger<JwtGeneratorService> _logger;
		private bool _disposed;

		public JwtGeneratorService(IHostEnvironment env, IOptions<CbarApiSettings> options, ILogger<JwtGeneratorService> logger)
		{
			_logger = logger;

			var settings = options.Value;

			var privateKeyPath = Path.Combine(env.ContentRootPath, "PrivateKey", settings.PrivateKeyFileName);

			if (!File.Exists(privateKeyPath))
			{
				throw new FileNotFoundException($"Private Key faylı tapılmadı: {privateKeyPath}", privateKeyPath);
			}

			var pem = File.ReadAllText(privateKeyPath);

			_rsa = RSA.Create();
			_rsa.ImportFromPem(pem);

			_logger.LogInformation("RSA private key uğurla yükləndi: {PrivateKeyPath}", privateKeyPath);
		}

		public string CreateJwtRs256(string issuer, string audienceDomain, TimeSpan lifetime)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(JwtGeneratorService));

			if (lifetime > TimeSpan.FromMinutes(15))
				throw new ArgumentException("Token lifetime maksimum 15 dəqiqə olmalıdır.");

			var nowUtc = DateTimeOffset.UtcNow;
			var nbfUtc = nowUtc.AddSeconds(-150);
			var expiresUtc = nowUtc.Add(lifetime);

			var key = new RsaSecurityKey(_rsa)
			{
				CryptoProviderFactory = new CryptoProviderFactory
				{
					CacheSignatureProviders = false
				}
			};

			var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

			var claims = new List<Claim>
			{
				new Claim(
					JwtRegisteredClaimNames.Iat,
					nowUtc.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64
				)
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

		public void Dispose()
		{
			if (_disposed)
				return;

			_rsa.Dispose();
			_disposed = true;
		}
	}
}