using Microsoft.AspNetCore.SignalR;

namespace Api
{
    public class ChatHub : Hub
    {
        private readonly ChatConnectionManager _connectionManager;

        public ChatHub(ChatConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            _connectionManager.RemoveConnection(connectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
