using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        [BindProperty]
        public ProductDTO? Product { get; set; } = new();

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                await client.DeleteAsync($"api/Products/{id}");

                _logger.LogInformation("Product Deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deleting Product");
            }

            return RedirectToPage("List");
        }
    }
}
