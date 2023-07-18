using Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var userMatch = _chatDBContextReader
                .tbl_users
                .AsNoTracking()
                .FirstOrDefault(
                u => u.username == auth.username
                && u.password == auth.password
                );
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
        public IActionResult ValidateToken()
        {
            var authResponse = new AuthResponse();

            var jwt = _jwtToken.ValidateAuthorizationHeader();
            if (jwt == null)
            {
                return BadRequest("Invalid authorization header or JWT token");
            }

            var accessToken = _jwtToken.GenerateJwtToken(jwt);
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
