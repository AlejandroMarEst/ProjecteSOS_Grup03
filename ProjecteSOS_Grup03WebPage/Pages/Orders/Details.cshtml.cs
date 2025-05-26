using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public List<ProductOrderDetailsDTO> Order { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                    ErrorMessage = "No s'ha trobat la comanda.";
                }
                else
                {
                    _logger.LogError("Order Loading Failed");
                    ErrorMessage = "Error en carregar la comanda.";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }

    }
}
