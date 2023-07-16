using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Web.Dtos;
using Web.Helper;

namespace Web
{
    public class JwtTokenValidator : JwtSecurityTokenHandler, ISecurityTokenValidator
    {
        private readonly JwtSetting _jwtSetting;
        private readonly IApiClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenValidator(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            _jwtSetting = serviceProvider.GetRequiredService<IOptions<JwtSetting>>().Value;
            _apiClient = serviceProvider.GetRequiredService<IApiClient>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        }

        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var response = _apiClient.SendRequestAsync(_jwtSetting.JwtTokenValidateUrl, HttpMethod.Get, null, token);
            var success = response.Result.success;
            var body = response.Result.body;
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(body)!;
            if (!success)
            {
                throw new SecurityTokenValidationException(authResponse.error_message);
            }

            var accessToken = authResponse.access_token;
            var session = _httpContextAccessor!.HttpContext!.Session;
            session.SetString(_jwtSetting.AuthKey, accessToken);

            var keyBytes = Encoding.UTF8.GetBytes(_jwtSetting.SigningKey);
            var key = new byte[16]; // 128 bits key size

            if (keyBytes.Length >= 16)
            {
                Array.Copy(keyBytes, key, 16);
            }
            else
            {
                throw new InvalidOperationException("The key size is too small for the HS256 encryption algorithm.");
            }
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
            return base.ValidateToken(token, validationParameters, out validatedToken);
        }
    }
}
