using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class OrderListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderListModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		public List<ProductOrderDetailsDTO> Orders { get; set; } = [];
        public bool OrderExists { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public double GrandTotal { get; private set; }

        public OrderListModel(IHttpClientFactory httpClientFactory, ILogger<OrderListModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
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

                    OrderExists = Orders.Any();

                    if (Orders.Any())
                    {
                        GrandTotal = Math.Round(Orders.Sum(order => order.TotalPrice), 2);
                    }
                    else
                    {
                        GrandTotal = 0;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    OrderExists = false;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    if(error != "No hi ha cap comanda activa")
                    {
                        _logger.LogError(await response.Content.ReadAsStringAsync());
                        ErrorMessage = _localizer["LoadingCartError"];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Orders");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }
        }
    }
}
