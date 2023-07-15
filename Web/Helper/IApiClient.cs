using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Web.Helper
{
    public interface IApiClient
    {
        Task<(string body, bool success, int status)> SendRequestAsync(string uri, HttpMethod method, string? content = null, string? bearerToken = null);
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(string body, bool success, int status)> SendRequestAsync(string uri, HttpMethod method, string? content = null, string? bearerToken = null)
        {
            var request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(bearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            request.Headers.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(content))
            {
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var isSuccess = response.IsSuccessStatusCode;

            if (!isSuccess)
            {
                var error = new ApiClientError
                {
                    Uri = uri,
                    Method = method.Method,
                    ResponseBody = responseBody,
                    BearerToken = bearerToken,
                    StatusCode = response.StatusCode,
                };
                _logger.LogError(JsonSerializer.Serialize(error));
            }

            return (responseBody, isSuccess, (int)response.StatusCode);
        }
    }

    public class ApiClientError
    {
        public string Uri { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string ResponseBody { get; set; } = string.Empty;
        public string? BearerToken { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }
    }
}
