using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public ProductDTO NewProduct { get; set; }
        public string? ErrorMessage { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
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

                if (TokenHelper.IsTokenSession())
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Creating Product");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }
    }
}
