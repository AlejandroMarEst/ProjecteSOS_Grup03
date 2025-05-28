using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net;
using ProjecteSOS_Grup03WebPage.DTOs;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class AddToCartModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AddToCartModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

        [BindProperty]
        public int Quantity { get; set; } = 0;
        public int ProductId { get; set; }

        public string? ErrorMessage { get; set; }

        public AddToCartModel(IHttpClientFactory httpClientFactory, ILogger<AddToCartModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
			_localizer = localizer;
		}

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ProductId = id;

            _logger.LogInformation("Product Id {ProductId} defined", id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync("api/OrderedProducts", new ProductOrderCreateDTO { ProductId = id, Quantity = Quantity} );

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("List");
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    ErrorMessage = _localizer["ProductAlreadyInCart"];
                }
                else
                {
                    ErrorMessage = _localizer["AddToCartApiError"] + response.StatusCode;
                }

                _logger.LogInformation($"Product Added To Order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Adding Product To Order");
            }

            return Page();
        }

    }
}
