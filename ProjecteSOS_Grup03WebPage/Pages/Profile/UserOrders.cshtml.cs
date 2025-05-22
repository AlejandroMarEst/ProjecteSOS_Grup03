using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Pages.Products;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class UserOrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserOrdersModel> _logger;

        public List<OrderListDTO> Orders { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public UserOrdersModel(IHttpClientFactory httpClientFactory, ILogger<UserOrdersModel> logger)
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
                    var orders = JsonSerializer.Deserialize<List<OrderListDTO>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                    Orders = orders ?? new List<OrderListDTO>();
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
