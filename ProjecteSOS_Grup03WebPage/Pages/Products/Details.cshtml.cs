using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public ProductListDTO? Product { get; set; }
        public string? ErrorMessage { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var response = await client.GetAsync($"api/Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Product = JsonSerializer.Deserialize<ProductListDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (Product == null)
                    {
                        ErrorMessage = "No s'ha trobat el producte.";
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = "No s'ha trobat el producte.";
                }
                else
                {
                    _logger.LogError("Product Loading Failed");
                    ErrorMessage = "Error en carregar el producte.";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }
    }
}
