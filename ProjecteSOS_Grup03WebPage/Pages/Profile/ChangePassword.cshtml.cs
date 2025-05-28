using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using ProjecteSOS_Grup03WebPage.DTOs;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChangePasswordModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public string OldPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public string? ErrorMessage { get; set; }

        public ChangePasswordModel(IHttpClientFactory httpClientFactory, ILogger<ChangePasswordModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
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

                var response = await client.PatchAsJsonAsync<object>($"api/Auth/Profile/UpdatePassword?oldPasswd={OldPassword}&newPasswd={NewPassword}", new { });
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("../Profile");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = _localizer["UnauthorizedPasswordChange"];
                }
                else
                {
                    ErrorMessage = _localizer["PasswordChangeFailedBadRequest"] + response.StatusCode;
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
