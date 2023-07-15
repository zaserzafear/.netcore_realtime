using Microsoft.AspNetCore.SignalR;

namespace Api
{
    public class SignalRHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                await UpdateUserStatus(userName, isOnline: true);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                await UpdateUserStatus(userName, isOnline: false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserStatus(string userName, bool isOnline)
        {
            await Clients.All.SendAsync("UpdateUserStatus", userName, isOnline);
        }

        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
}
