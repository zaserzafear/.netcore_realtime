using Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtToken _jwtToken;
        private readonly ChatDBContextReader _chatDBContextReader;

        public AuthenticationController(JwtToken jwtToken, ChatDBContextReader chatDBContextReader)
        {
            _jwtToken = jwtToken;
            _chatDBContextReader = chatDBContextReader;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] AuthRequest auth)
        {
            var userMatch = _chatDBContextReader.tbl_users.FirstOrDefault(u => u.username == auth.username && u.password == auth.password);
            var authResponse = new AuthResponse();

            if (userMatch != null)
            {
                var accessToken = _jwtToken.GenerateJwtToken(new JwtTokenBuildDto
                {
                    name = userMatch.username,
                    sub = userMatch.user_id.ToString(),
                });

                authResponse.access_token = accessToken;
                return Ok(authResponse);
            }
            else
            {
                authResponse.error_message = "Authentication failed";
                return Unauthorized(authResponse);
            }
        }

        [HttpGet("ValidateToken")]
        public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return BadRequest("Invalid authorization header");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            var dto = _jwtToken.ExtractJwtTokenFromHeader(token);
            var authResponse = new AuthResponse();

            if (dto == null)
            {
                authResponse.error_message = "Unable to extract token information";
                return BadRequest(authResponse);
            }

            var accessToken = _jwtToken.GenerateJwtToken(dto);

            if (string.IsNullOrEmpty(accessToken))
            {
                authResponse.error_message = "Unable to generate new token";
                return BadRequest(authResponse);
            }

            authResponse.access_token = accessToken;
            return Ok(authResponse);
        }
    }
}
