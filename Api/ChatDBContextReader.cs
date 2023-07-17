using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class ChatDBContextReader : ChatDBContext
    {
        public ChatDBContextReader(string connectionString, string serverVersion) : base(GetOptions(connectionString, serverVersion))
        {
        }

        private static DbContextOptions<ChatDBContext> GetOptions(string connectionString, string serverVersion)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatDBContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.Parse(serverVersion));
            return optionsBuilder.Options;
        }
    }
}
