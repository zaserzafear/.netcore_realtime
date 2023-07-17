using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class ChatDBContextWriter : ChatDBContext
    {
        public ChatDBContextWriter(string connectionString, string serverVersion) : base(GetOptions(connectionString, serverVersion))
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
