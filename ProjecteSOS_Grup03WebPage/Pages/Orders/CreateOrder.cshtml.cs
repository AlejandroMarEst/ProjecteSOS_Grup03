using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class CreateOrderModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateOrderModel> _logger;

        public CreateOrderModel(IHttpClientFactory httpClientFactory, ILogger<CreateOrderModel> logger)
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

                await client.PostAsJsonAsync<object?>("api/Orders/NewOnlineOrder", null);

                _logger.LogInformation("Order Confirmed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Confirming Product");
            }

            return RedirectToPage("OrderList");
        }
    }
}
