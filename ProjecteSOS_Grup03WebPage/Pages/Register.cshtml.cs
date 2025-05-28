using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger<RegisterModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public RegisterDTO Register { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public RegisterModel(IHttpClientFactory httpClient, ILogger<RegisterModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localizer = localizer;
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
                                ErrorMessage = _localizer["EmailAlreadyRegistered"];
                                break;
                            }
                            else if (error.Code.StartsWith("Password"))
                            {
                                ErrorMessage = _localizer["PasswordRequirementsError"]; // Shows the password requirement message
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
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }
            return Page();
        }
    }
}
