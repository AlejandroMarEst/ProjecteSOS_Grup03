using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class ProductsListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public List<ProductListDTO> Products { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public ProductsListModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var response = await client.GetAsync("api/Products");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductListDTO>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                    Products = products ?? new List<ProductListDTO>();
                }
                else
                {
                    _logger.LogError("Products Loading Failed");
                    ErrorMessage = "Loading Products Error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Products");
                ErrorMessage = "There was an unexpected error. Try again.";
            }
        }
    }
}
