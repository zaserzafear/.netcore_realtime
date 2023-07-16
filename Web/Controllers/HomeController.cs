using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Web.Dtos;
using Web.Helper;
using Web.Models;
using Web.Settings;

namespace Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiClient _apiClient;
        private readonly ServiceUrlSetting _serviceUrlSetting;
        private readonly JwtSetting _jwtSetting;

        public HomeController(ILogger<HomeController> logger, IApiClient apiClient, IOptions<ServiceUrlSetting> serviceUrlSetting, IOptions<JwtSetting> jwtSetting)
        {
            _logger = logger;
            _apiClient = apiClient;
            _serviceUrlSetting = serviceUrlSetting.Value;
            _jwtSetting = jwtSetting.Value;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(_jwtSetting.AuthKey)))
            {
                return RedirectToAction("Chat");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var authResponse = await IsValidUser(username, password);

            if (!string.IsNullOrEmpty(authResponse?.access_token))
            {
                HttpContext.Session.SetString(_jwtSetting.AuthKey, authResponse.access_token);

                return RedirectToAction("Chat");
            }
            else
            {
                ViewBag.ErrorMessage = authResponse?.error_message;
                return View("Index");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(_jwtSetting.AuthKey);

            return RedirectToAction("Index");
        }

        public IActionResult Chat()
        {
            var authKey = HttpContext.Session.GetString(_jwtSetting.AuthKey);
            ViewData["IsLoggedIn"] = !string.IsNullOrEmpty(authKey);

            return View();
        }

        [HttpGet]
        public IActionResult GetChatHubUrl()
        {
            var chatHubUrl = _serviceUrlSetting.ChatHub;
            return Ok(new GetChatHubUrl { chatHubUrl = chatHubUrl });
        }

        private async Task<AuthResponse?> IsValidUser(string username, string password)
        {
            var authRequest = new AuthRequest
            {
                username = username,
                password = password,
            };

            var requestBody = JsonSerializer.Serialize(authRequest);
            var (responseBody, success, status) = await _apiClient.SendRequestAsync($"{_serviceUrlSetting.AuthApi}/Login", HttpMethod.Post, requestBody);
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseBody);

            return authResponse;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}