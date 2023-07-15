using System.Collections.Concurrent;

namespace Api
{
    public class ConnectionManager
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedClients = new ConcurrentDictionary<string, string>();

        public void AddConnection(string connectionId, string userId)
        {
            ConnectedClients.TryAdd(connectionId, userId);
        }

        public void RemoveConnection(string connectionId)
        {
            ConnectedClients.TryRemove(connectionId, out _);
        }

        public string? GetUserByConnectionId(string connectionId)
        {
            ConnectedClients.TryGetValue(connectionId, out var userId);
            return userId;
        }
    }
}
