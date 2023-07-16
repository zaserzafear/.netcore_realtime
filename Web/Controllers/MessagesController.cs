using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Web.Dtos;
using Web.Helper;
using Web.Settings;

namespace Web.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IApiClient _apiClient;
        private readonly ServiceUrlSetting _serviceUrlSetting;
        private readonly JwtSetting _jwtSetting;

        public MessagesController(ILogger<MessagesController> logger, IApiClient apiClient, IOptions<ServiceUrlSetting> serviceUrlSetting, IOptions<JwtSetting> jwtSetting)
        {
            _logger = logger;
            _apiClient = apiClient;
            _serviceUrlSetting = serviceUrlSetting.Value;
            _jwtSetting = jwtSetting.Value;
        }

        [HttpPost]
        public async Task<IActionResult> SetConnectionIdTouser([FromBody] SignalRConnectionId signalRConnectionId)
        {
            var accessToken = HttpContext.Session.GetString(_jwtSetting.AuthKey);
            var requestBody = JsonSerializer.Serialize(signalRConnectionId);

            var response = await _apiClient.SendRequestAsync($"{_serviceUrlSetting.ChatApi}/SetConnectionIdTouser", HttpMethod.Post, requestBody, accessToken);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto sendMessageDto)
        {
            var accessToken = HttpContext.Session.GetString(_jwtSetting.AuthKey);
            var requestBody = JsonSerializer.Serialize(sendMessageDto);

            var response = await _apiClient.SendRequestAsync($"{_serviceUrlSetting.ChatApi}/SendMessage", HttpMethod.Post, requestBody, accessToken);

            return Ok();
        }
    }
}
