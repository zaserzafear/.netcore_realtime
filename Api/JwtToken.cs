using Api.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api
{
    public class JwtToken
    {
        private readonly JwtSetting _jwtSetting;
        private readonly ILogger<JwtToken> _logger;

        public JwtToken(IOptions<JwtSetting> jwtSetting, ILogger<JwtToken> logger)
        {
            _jwtSetting = jwtSetting.Value;
            _logger = logger;
        }

        public string GenerateJwtToken(JwtTokenBuildDto dto)
        {
            var jti = Guid.NewGuid().ToString();
            var jwtIssuer = _jwtSetting.Issuer;
            var jwtAudience = _jwtSetting.Audience;
            var jwtKey = _jwtSetting.SigningKey;
            var jwtExpires = _jwtSetting.ExpirationMinutes;

            var now = DateTimeOffset.UtcNow;
            var expires = now.AddMinutes(jwtExpires);

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var key = new byte[16]; // 128 bits key size

            if (keyBytes.Length >= 16)
            {
                Array.Copy(keyBytes, key, 16);
            }
            else
            {
                throw new InvalidOperationException("The key size is too small for the HS256 encryption algorithm.");
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(key);
            var creds = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.NameIdentifier, dto.sub),
                new Claim(ClaimTypes.Name, dto.name),
            };

            foreach (var role in dto.roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenOptions = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: expires.UtcDateTime,
                signingCredentials: creds
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return token;
        }

        public JwtTokenBuildDto? ExtractJwtTokenFromHeader(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var sub = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
                var name = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
                var roles = jwtToken.Claims.Where(claim => claim.Type == ClaimTypes.Role)?.Select(claim => claim.Value).ToList();

                var dto = new JwtTokenBuildDto
                {
                    sub = sub!,
                    name = name!,
                    roles = roles!,
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
