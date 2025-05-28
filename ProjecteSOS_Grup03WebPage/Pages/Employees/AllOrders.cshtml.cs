using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Pages.Orders;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Employees
{
    public class AllOrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderListModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		public List<OrderListDTO> Orders { get; set; } = [];
        public List<UserProfileDTO> Users { get; set;} = [];
        public string? ErrorMessage { get; set; }
        public AllOrdersModel(IHttpClientFactory httpClientFactory, ILogger<OrderListModel> logger, IStringLocalizer<SharedResource> localizer)
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

                var response = await client.GetAsync("api/Orders");
                var response2 = await client.GetAsync("api/Auth/Profiles");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<OrderListDTO>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    var json2 = await response2.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserProfileDTO>>(json2, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    Orders = orders ?? new List<OrderListDTO>();
                    Users = users ?? new List<UserProfileDTO>();
                }
                else
                {
                    _logger.LogError("Orders Loading Failed");
                    ErrorMessage = _localizer["LoadingAllOrdersError"];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Orders");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain."];
            }
        }
    }
}
