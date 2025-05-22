using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileModel> _logger;

        public UserProfileDTO? Profile { get; set; }
        public string? ErrorMessage { get; set; }

        public ProfileModel(IHttpClientFactory httpClient, ILogger<ProfileModel> logger)
        {
            _httpClientFactory = httpClient;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");
                var response = await client.GetAsync("api/Auth/Profile");

                if (TokenHelper.IsTokenSession(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Profile = JsonSerializer.Deserialize<UserProfileDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (Profile == null)
                    {
                        ErrorMessage = "No s'ha trobat l'usuari.";
                    }
                }
                else
                {
                    _logger.LogError("User Profile Loading Failed");
                    ErrorMessage = "Loading Profile Error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading User Profile");
                ErrorMessage = "There was an unexpected error. Try again.";
            }
        }
    }
}
