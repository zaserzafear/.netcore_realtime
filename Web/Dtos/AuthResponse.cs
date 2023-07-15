namespace Web.Dtos
{
    public class AuthResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string? error_message { get; set; }
    }
}
