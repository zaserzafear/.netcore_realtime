using System.Collections.Concurrent;

namespace Api
{
    public class ChatConnectionManager
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedClients = new ConcurrentDictionary<string, string>();

        public void AddConnection(string connectionId, string userId)
        {
            ConnectedClients.TryAdd(connectionId, userId);
        }

        public string? GetUserByConnectionId(string connectionId)
        {
            ConnectedClients.TryGetValue(connectionId, out var userId);
            return userId;
        }

        public IEnumerable<string> GetConnectionsByUserId(string userId)
        {
            return ConnectedClients.Where(entry => entry.Value == userId).Select(entry => entry.Key);
        }

        public void RemoveConnection(string connectionId)
        {
            ConnectedClients.TryRemove(connectionId, out _);
        }

        public IDictionary<string, string> GetAllConnections()
        {
            return ConnectedClients.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

    }
}
