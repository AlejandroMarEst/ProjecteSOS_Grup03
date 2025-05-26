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
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public OrderProductCreateDTO OrderProductCreate { get; set; }
        public string? ErrorMessage { get; set; }

        public AddToCartModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
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
                var token = HttpContext.Session.GetString("AuthToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync("api/OrderedProducts", OrderProductCreate);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage($"Details/{OrderProductCreate.ProductId}");
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

            return RedirectToPage($"Details/{OrderProductCreate.ProductId}");
        }

    }
}
