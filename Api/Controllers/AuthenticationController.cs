using Api.Dtos;
using Api.Models;
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
        private readonly List<AuthModel> _usrList;

        public AuthenticationController(JwtToken jwtToken)
        {
            _jwtToken = jwtToken;

            _usrList = new List<AuthModel>();
            for (uint i = 1; i <= 5; i++)
            {
                _usrList.Add(new AuthModel { id = i, username = $"user{i}", password = $"password{i}" });
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] AuthRequest auth)
        {
            var userMatch = _usrList.FirstOrDefault(u => u.username == auth.username && u.password == auth.password);
            var authResponse = new AuthResponse();

            if (userMatch != null)
            {
                authResponse.access_token = _jwtToken.GenerateJwtToken(new JwtTokenBuildDto
                {
                    name = userMatch.username,
                    sub = userMatch.id.ToString(),
                });

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
            var combinedKey = _jwtToken.ExtractCombinedKey(token);

            if (string.IsNullOrEmpty(combinedKey))
            {
                return BadRequest("Invalid token");
            }

            var dto = _jwtToken.ExtractJwtTokenFromHeader(token);

            if (dto == null)
            {
                return BadRequest("Unable to extract token information");
            }

            var newToken = _jwtToken.GenerateJwtToken(dto);

            if (string.IsNullOrEmpty(newToken))
            {
                return BadRequest("Unable to generate new token");
            }

            return Ok(new
            {
                combinedKey = combinedKey,
                newToken = newToken
            });
        }
    }
}
