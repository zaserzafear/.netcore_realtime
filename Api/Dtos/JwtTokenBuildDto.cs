namespace Api.Dtos
{
    public class JwtTokenBuildDto
    {
        public string sub { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public List<string> roles { get; set; } = new List<string>();
    }
}
