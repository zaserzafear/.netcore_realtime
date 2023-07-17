namespace Api
{
    public class RedisSetting
    {
        public RedisChat Chat { get; set; } = new RedisChat();
    }

    public class RedisChat
    {
        public string ConnectedClientsKey { get; set; } = string.Empty;
        public string ConnectionStrings { get; set; } = string.Empty;
    }
}
