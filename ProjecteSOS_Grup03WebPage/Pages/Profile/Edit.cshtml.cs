using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        public string? ErrorMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                        ErrorMessage = "No s'ha trobat l'usuari.";
                    }
                    else
                    {
                        Name = user.Name;
                        Phone = user.Phone;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = "No s'ha trobat l'usuari.";
                }
                else
                {
                    _logger.LogError("User Loading Failed");
                    ErrorMessage = "Error en carregar l'usuari.";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading User");
                ErrorMessage = "There was an unexpected error. Try again.";
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
