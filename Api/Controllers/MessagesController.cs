using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly ConnectionManager _connectionManager;

        public MessagesController(IHubContext<SignalRHub> hubContext, ConnectionManager connectionManager)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        [HttpPost("sendToAll")]
        public async Task<IActionResult> SendMessageToAll(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [HttpPost("sendToGroup")]
        public async Task<IActionResult> SendMessageToGroup(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [HttpPost("sendToUser")]
        public async Task<IActionResult> SendMessageToUser(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [HttpPost("disconnect/{connectionId}")]
        public IActionResult DisconnectClient(string connectionId)
        {
            var userId = _connectionManager.GetUserByConnectionId(connectionId);
            if (userId != null)
            {
                _hubContext.Clients.Client(connectionId).SendAsync("Disconnect");
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
