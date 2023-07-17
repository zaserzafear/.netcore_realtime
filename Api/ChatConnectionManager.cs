using StackExchange.Redis;

namespace Api
{
    public class ChatConnectionManager
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly string connectedClientsKey = "";

        public ChatConnectionManager(string connectedClientsKey, string connectionString)
        {
            this.connectedClientsKey = connectedClientsKey;
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
        }

        public async Task AddConnection(string connectionId, string userId)
        {
            await _database.HashSetAsync(connectedClientsKey, connectionId, userId);
        }

        public async Task<string?> GetUserByConnectionId(string connectionId)
        {
            var userId = await _database.HashGetAsync(connectedClientsKey, connectionId);
            return userId.HasValue ? userId.ToString() : null;
        }

        public async Task<IEnumerable<string>> GetConnectionsByUserId(string userId)
        {
            var connections = await _database.HashKeysAsync(connectedClientsKey);
            return connections.Where(connection => _database.HashGet(connectedClientsKey, connection) == userId).Select(connection => connection.ToString());
        }

        public async Task RemoveConnection(string connectionId)
        {
            await _database.HashDeleteAsync(connectedClientsKey, connectionId);
        }

        public async Task<IDictionary<string, string>> GetAllConnections()
        {
            var connections = await _database.HashGetAllAsync(connectedClientsKey);
            return connections.ToDictionary(entry => entry.Name.ToString(), entry => entry.Value.ToString());
        }
    }
}
