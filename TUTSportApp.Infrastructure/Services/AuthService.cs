using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TUTSportApp.Application.Common.Interfaces;
using TUTSportApp.Application.Common.Models;
using TUTSportApp.Application.Features.Auth.Commands;
using System.Buffers.Binary;

namespace TUTSportApp.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILoginRepository _loginRepository;

        // PBKDF2 parameters
        private const int SaltSize = 16;       // 128-bit salt
        private const int HashSize = 32;       // 256-bit hash
        private const int DefaultIterations = 200_000;

        public AuthService(IOptions<JwtSettings> jwtSettings, ILoginRepository loginRepository)
        {
           ArgumentNullException.ThrowIfNull(jwtSettings);
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings), "Value cannot be null.");
            _loginRepository = loginRepository ?? throw new ArgumentNullException(nameof(loginRepository));

            if (string.IsNullOrWhiteSpace(_jwtSettings.Key))
            {
                throw new ArgumentException("JWT signing key is required.", nameof(jwtSettings));
            }

            if (_jwtSettings.DurationInMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jwtSettings), "Token duration must be positive.");
            }
        }

        public async Task<string> CreateTokenAsync(LoginCommand request)
        {
           ArgumentNullException.ThrowIfNull(request);

            var login = await _loginRepository
                .GetByUsernameAsync(request.Username)
                .ConfigureAwait(false);

            if (login is null)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, login.UserId.ToString()),
                new Claim(ClaimTypes.Name, login.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Format: "{iterations}.{saltBase64}.{hashBase64}"
        public string CreatePasswordHash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));
            }

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                DefaultIterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{DefaultIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPasswordHash(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            var parts = passwordHash.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out var iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;

            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                // More specific than catch-all; satisfies analyzer
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}

