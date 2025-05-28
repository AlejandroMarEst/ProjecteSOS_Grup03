using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public ProductOrderDTO? ProductOrder { get; set; }
        public string? ErrorMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger, IStringLocalizer<SharedResource> localizer)
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

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"api/OrderedProducts/User/CurrentOrder/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    ProductOrder = JsonSerializer.Deserialize<ProductOrderDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (ProductOrder == null)
                    {
                        ErrorMessage = _localizer["OrderItemNotFound"];
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = _localizer["OrderItemNotFound"];
                }
                else
                {
                    _logger.LogError("Product Loading Failed");
                    ErrorMessage = _localizer["UnexcpectedErrorTryAgain"] + response.StatusCode;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
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
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                if (ProductOrder == null)
                {
                    ErrorMessage = _localizer["OrderItemNotFound"];
                    return Page();
                }

                var response = await client.PatchAsJsonAsync($"api/OrderedProducts/Quantity/{id}?productId={id}&newQuantity={ProductOrder.Quantity}", new { });

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("OrderList");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = _localizer["UnauthorizedEditOrderItem"];
                }
                else
                {
                    ErrorMessage = _localizer["OrderItemOrOrderNotFoundForEdit"] + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Editing Product");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
