using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileModel> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public UserProfileDTO? Profile { get; set; }
        public string? ErrorMessage { get; set; }

        public ProfileModel(IHttpClientFactory httpClient, ILogger<ProfileModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClient;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (TokenHelper.IsTokenSession(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("api/Auth/Profile");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Profile = JsonSerializer.Deserialize<UserProfileDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (Profile == null)
                    {
                        ErrorMessage = _localizer["UserProfileNotFound"];
                    }
                }
                else
                {
                    _logger.LogError("User Profile Loading Failed");
                    ErrorMessage = _localizer["LoadingUserProfileError"];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading User Profile");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }
        }
    }
}
