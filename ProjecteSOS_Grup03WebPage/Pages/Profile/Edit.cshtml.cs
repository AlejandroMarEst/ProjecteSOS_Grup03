using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        public string? ErrorMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<IActionResult> OnGetAsync(int id)
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
                    var user = JsonSerializer.Deserialize<UserProfileDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (user == null)
                    {
                        ErrorMessage = _localizer["UserProfileNotFound"];
                    }
                    else
                    {
                        Name = user.Name;
                        Phone = user.Phone;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = _localizer["UserProfileNotFound"];
                }
                else
                {
                    _logger.LogError("User Loading Failed");
                    ErrorMessage = _localizer["LoadingUserProfileError"];
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading User");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PutAsJsonAsync<object>($"api/Auth/Profile?name={Name}&phone={Phone}", new { });
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("../Profile");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = _localizer["UnauthorizedProfileEdit"];
                }
                else
                {
                    ErrorMessage = _localizer["EditProfileApiError"] + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Editing User Profile");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
