﻿using Api.Dtos;
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
        public async Task<IActionResult> SetConnectionIdTouser([FromBody] SignalRConnectionId signalRConnectionId)
        {
            var jwt = _jwtToken.ValidateAuthorizationHeader();
            if (jwt == null)
            {
                return BadRequest("Invalid authorization header or JWT token");
            }

            var userId = jwt.sub;
            var connectionId = signalRConnectionId.connectionId;

            await _connectionManager.AddConnection(connectionId, userId);

            return Ok(signalRConnectionId);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            var jwt = _jwtToken.ValidateAuthorizationHeader();
            if (jwt == null)
            {
                return BadRequest("Invalid authorization header or JWT token");
            }

            var userName = jwt.name;

            var sendToUser = messageDto.sendToUserInput;
            var message = messageDto.message;

            var connectionIds = await _connectionManager.GetConnectionsByUserId(sendToUser);

            var sendTasks = connectionIds.Select(connectionId =>
                _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", userName, message));

            await Task.WhenAll(sendTasks);

            return Ok();
        }

        [HttpDelete("Disconnect")]
        public async Task<IActionResult> DisconnectAsync(string connectionId)
        {
            var userId = await _connectionManager.GetUserByConnectionId(connectionId);
            if (!string.IsNullOrEmpty(userId))
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
