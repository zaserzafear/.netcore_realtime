using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Web
{
    internal class JwtTokenEvent : JwtBearerEvents
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSetting _jwtSetting;

        public JwtTokenEvent(IHttpContextAccessor httpContextAccessor, IOptions<JwtSetting> jwtSetting)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtSetting = jwtSetting.Value;
        }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            var token = _httpContextAccessor?.HttpContext?.Session.GetString(_jwtSetting.AuthKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            return base.AuthenticationFailed(context);
        }

        public override Task Challenge(JwtBearerChallengeContext context)
        {
            return base.Challenge(context);
        }
    }
}