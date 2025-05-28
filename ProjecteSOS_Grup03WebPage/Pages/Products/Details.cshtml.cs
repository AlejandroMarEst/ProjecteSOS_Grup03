using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		public ProductListDTO? Product { get; set; }
        public string? ErrorMessage { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
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
                        ErrorMessage = _localizer["ProductNotFound"];
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = _localizer["ProductNotFound"];
                }
                else
                {
                    _logger.LogError("Product Loading Failed");
                    ErrorMessage = _localizer["LoadingProductDetailsError"];
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
