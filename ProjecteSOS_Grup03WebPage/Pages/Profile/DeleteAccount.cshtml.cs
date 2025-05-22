using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.Pages.Products;
using System.Net.Http.Headers;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class DeleteAccountModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteAccountModel> _logger;

        public DeleteAccountModel(IHttpClientFactory httpClientFactory, ILogger<DeleteAccountModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                await client.DeleteAsync($"api/Auth/Profile/DeleteAccount");

                _logger.LogInformation("Account Deleted");

                HttpContext.Session.Remove("AuthToken");
                HttpContext.Session.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deleting Account");
            }

            return RedirectToPage("/Index");
        }
    }
}
