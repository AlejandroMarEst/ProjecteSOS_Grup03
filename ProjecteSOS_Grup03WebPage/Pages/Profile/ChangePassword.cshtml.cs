using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using ProjecteSOS_Grup03WebPage.DTOs;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChangePasswordModel> _logger;

        [BindProperty]
        public string OldPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public string? ErrorMessage { get; set; }

        public ChangePasswordModel(IHttpClientFactory httpClientFactory, ILogger<ChangePasswordModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                    ErrorMessage = "Només pots editar el teu propi perfil";
                }
                else
                {
                    ErrorMessage = "Error en editar el perfil: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Editing User Profile");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }
    }
}
