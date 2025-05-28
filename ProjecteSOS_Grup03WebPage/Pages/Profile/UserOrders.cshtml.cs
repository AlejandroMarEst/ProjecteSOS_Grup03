using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Pages.Products;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Profile
{
    public class UserOrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserOrdersModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		public List<UserOrderListDTO> Orders { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public UserOrdersModel(IHttpClientFactory httpClientFactory, ILogger<UserOrdersModel> logger, IStringLocalizer<SharedResource> localizer)
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
                var response = await client.GetAsync("api/Orders/User");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<UserOrderListDTO>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                    Orders = orders ?? new List<UserOrderListDTO>();
                }
                else
                {
                    _logger.LogError("Orders Loading Failed");
                    ErrorMessage = _localizer["LoadingUserOrdersError"];
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
