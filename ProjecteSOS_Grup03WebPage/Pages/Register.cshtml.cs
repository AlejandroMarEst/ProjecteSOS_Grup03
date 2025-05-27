using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Text.Json;

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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errors = JsonSerializer.Deserialize<List<IdentityError>>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errors != null)
                    {
                        foreach (var error in errors)
                        {
                            if (error.Code == "DuplicateUserName")
                            {
                                ErrorMessage = "This email is already registered.";
                                break;
                            }
                            else if (error.Code.StartsWith("Password"))
                            {
                                ErrorMessage = "The password must be longer than 8 character and contain Majus and special characters"; // Shows the password requirement message
                                break;
                            }
                        }
                        _logger.LogInformation(ErrorMessage);
                    }
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
