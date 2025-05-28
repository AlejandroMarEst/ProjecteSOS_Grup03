using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public LoginDTO Login { get; set; } = new LoginDTO();
        public string? ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var response = await client.PostAsJsonAsync("api/Auth/login", Login);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(token))
                    {
                        HttpContext.Session.SetString("AuthToken", token);
                        _logger.LogInformation("Login susccesfull");
                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    _logger.LogError(errorContent, "Login failed");
                    ErrorMessage = _localizer["IncorrectLogin"];

                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
