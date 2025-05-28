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
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		public List<ProductOrderDetailsDTO> Order { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (TokenHelper.IsTokenSession(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"api/OrderedProducts/User/ForOrder/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<List<ProductOrderDetailsDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Order = order ?? new List<ProductOrderDetailsDTO>();
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = _localizer["OrderNotFound"];
                }
                else
                {
                    _logger.LogError("Order Loading Failed");
                    ErrorMessage = _localizer["LoadingOrderDetailsError"];
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
