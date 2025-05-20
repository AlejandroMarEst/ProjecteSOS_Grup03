using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger<RegisterModel> _logger;
        [BindProperty]
        public RegisterDTO Register { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public RegisterModel(IHttpClientFactory httpClient, ILogger<RegisterModel> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();
            try
            {
                var client = _httpClient.CreateClient("SosApi");
                var response = await client.PostAsJsonAsync("api/Auth/register", Register);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Register susccesfull");
                    return RedirectToPage("/Index");
                }
                else
                {
                    _logger.LogInformation("Register failed");
                    ErrorMessage = "Registry error.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registry error");
                ErrorMessage = "There was an unexpected error. Try again.";
            }
            return Page();
        }
    }
}
