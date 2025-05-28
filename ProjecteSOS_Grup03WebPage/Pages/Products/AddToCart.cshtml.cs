using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Net;
using ProjecteSOS_Grup03WebPage.DTOs;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class AddToCartModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AddToCartModel> _logger;

        [BindProperty]
        public int Quantity { get; set; } = 0;
        public int ProductId { get; set; }
        //public ProductOrderCreateDTO OrderProductCreate { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public AddToCartModel(IHttpClientFactory httpClientFactory, ILogger<AddToCartModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            //OrderProductCreate.ProductId = id;
            ProductId = id;

            _logger.LogInformation("Product Id {ProductId} defined", id);

            return Page();
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
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync("api/OrderedProducts", new ProductOrderCreateDTO { ProductId = ProductId, Quantity = Quantity} );

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("List");
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    ErrorMessage = "Aquest producte ja està en la comanda, edita la quantitat";
                }
                else
                {
                    ErrorMessage = "Error en afegir un producte a una ordre: " + response.StatusCode;
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
