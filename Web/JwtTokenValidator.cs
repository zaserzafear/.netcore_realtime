using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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

            if (!success)
            {
                throw new SecurityTokenValidationException(body);
            }

            var combinedKey = JsonSerializer.Deserialize<Dictionary<string, object>>(body)?["combinedKey"].ToString()!;
            var newToken = JsonSerializer.Deserialize<Dictionary<string, object>>(body)?["newToken"].ToString()!;
            var session = _httpContextAccessor!.HttpContext!.Session;
            session.SetString(_jwtSetting.AuthKey, newToken);

            var bytes = Encoding.UTF8.GetBytes(combinedKey);
            var base64String = Convert.ToBase64String(bytes);
            var keyBytes = Encoding.UTF8.GetBytes(base64String);
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
