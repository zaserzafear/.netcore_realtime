namespace Web
{
    public class JwtSetting
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public string JwtTokenValidateUrl { get; set; } = string.Empty;
        public string AuthKey { get; set; } = string.Empty;
    }
}
