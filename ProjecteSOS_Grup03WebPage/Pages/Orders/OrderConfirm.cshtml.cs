using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class OrderConfirmModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderConfirmModel> _logger;

        public OrderConfirmModel(IHttpClientFactory httpClientFactory, ILogger<OrderConfirmModel> logger)
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

                await client.PatchAsJsonAsync<object?>("api/Orders/ConfirmOnlineOrder", null);

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
