namespace Api
{
    public class JwtSetting
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public uint ExpirationMinutes { get; set; }
        public string RequestQuery { get; set; } = string.Empty;
    }
}
