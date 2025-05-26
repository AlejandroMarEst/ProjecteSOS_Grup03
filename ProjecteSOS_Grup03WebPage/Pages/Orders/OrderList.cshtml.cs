using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class OrderListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderListModel> _logger;

        public List<ProductOrderDetailsDTO> Orders { get; set; } = [];
        public bool OrderExists { get; set; } = false;
        public string? ErrorMessage { get; set; }

        public OrderListModel(IHttpClientFactory httpClientFactory, ILogger<OrderListModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (TokenHelper.IsTokenSession(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var response = await client.GetAsync("api/OrderedProducts/User/CurrentOrder");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<ProductOrderDetailsDTO>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                    Orders = orders ?? new List<ProductOrderDetailsDTO>();

                    OrderExists = true;
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    OrderExists = false;
                }
                else
                {
                    _logger.LogError("Orders Loading Failed");
                    ErrorMessage = "Loading Orders Error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Orders");
                ErrorMessage = "There was an unexpected error. Try again.";
            }
        }
    }
}
