using Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly JwtToken _jwtToken;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ChatConnectionManager _connectionManager;

        public MessagesController(JwtToken jwtToken, IHubContext<ChatHub> hubContext, ChatConnectionManager connectionManager)
        {
            _jwtToken = jwtToken;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        [AllowAnonymous]
        [HttpGet("GetAllConnections")]
        public IActionResult GetAllConnections()
        {
            return Ok(_connectionManager.GetAllConnections());
        }

        [HttpPost("SetConnectionIdTouser")]
        public IActionResult SetConnectionIdTouser([FromHeader(Name = "Authorization")] string authorizationHeader, [FromBody] SignalRConnectionId signalRConnectionId)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return BadRequest("Invalid authorization header");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            var jwt = _jwtToken.ExtractJwtTokenFromHeader(token)!;
            var userId = jwt.sub;
            var connectionId = signalRConnectionId.connectionId;

            _connectionManager.AddConnection(connectionId, userId);

            return Ok(signalRConnectionId);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromHeader(Name = "Authorization")] string authorizationHeader, [FromBody] MessageDto messageDto)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return BadRequest("Invalid authorization header");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            var jwt = _jwtToken.ExtractJwtTokenFromHeader(token)!;
            var userName = jwt.name;

            var sendToUser = messageDto.sendToUserInput;
            var message = messageDto.message;

            var connectionIds = _connectionManager.GetConnectionsByUserId(sendToUser);

            var sendTasks = connectionIds.Select(connectionId =>
                _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", userName, message));

            await Task.WhenAll(sendTasks);

            return Ok();
        }

        [HttpDelete("Disconnect")]
        public IActionResult Disconnect(string connectionId)
        {
            var userId = _connectionManager.GetUserByConnectionId(connectionId);
            if (userId != null)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
