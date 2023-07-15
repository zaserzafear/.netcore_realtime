using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Web
{
    internal class JwtTokenOptionsSetup
    {
        public void Configure(JwtBearerOptions options, IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var jwtSetting = serviceProvider.GetRequiredService<IOptions<JwtSetting>>().Value;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSetting.Issuer,
                ValidateIssuer = true,
                ValidAudience = jwtSetting.Audience,
                ValidateAudience = true,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.MaxValue,
                ValidateIssuerSigningKey = true
            };

            options.SecurityTokenValidators.Clear();
            options.SecurityTokenValidators.Add(new JwtTokenValidator(services));
        }
    }
}