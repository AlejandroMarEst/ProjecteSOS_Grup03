using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjecteSOS_Grup03API.Models;
using ProjecteSOS_Grup03API.Tools;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Pages.Orders;
using ProjecteSOS_Grup03WebPage.Tools;

namespace ProjecteSOS_Grup03WebPage.Pages.Employees
{
    public class SustainabilityStatsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderListModel> _logger;
        private readonly CsvSustainabilityService _csvService;

        [BindProperty]
        public SustainabilityRecord NewRecord { get; set; }
        public List<SustainabilityRecord> Records { get; set; }
        public int SustainableProductsPercent { get; set; } = 0;

        public SustainabilityStatsModel(IHttpClientFactory httpClientFactory, ILogger<OrderListModel> logger, CsvSustainabilityService csvService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _csvService = csvService;
        }

        public async Task OnGetAsync()
        {
            Records = _csvService.GetRecords();
            var client = _httpClientFactory.CreateClient("SosApi");
            var token = HttpContext.Session.GetString("AuthToken");

            if (TokenHelper.IsTokenSession(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("api/Orders");

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<OrderListDTO>>();
                if (orders != null)
                {
                    int allProducts = 0;
                    int sustainableProducts = 0;

                    var orderIds = orders.Select(order => order.OrderId).ToList();

                    foreach (var orderId in orderIds)
                    {
                        var responseProducts = await client.GetAsync($"api/OrderedProducts/ForOrder/{orderId}");

                        if (responseProducts.IsSuccessStatusCode)
                        {
                            var productDetails = await responseProducts.Content.ReadFromJsonAsync<List<ProductOrderDetailsDTO>>();
                            if (productDetails != null && productDetails.Any())
                            {
                                foreach (var productDetail in productDetails)
                                {
                                    allProducts += productDetail.Quantity;

                                    var productResponse = await client.GetAsync($"api/Products/{productDetail.ProductId}");

                                    if (productResponse.IsSuccessStatusCode)
                                    {
                                        var productData = await productResponse.Content.ReadFromJsonAsync<ProductDTO>();

                                        if (productData != null && productData.Points > 0)
                                        {
                                            sustainableProducts += productDetail.Quantity;
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Product {ProductId} for order {OrderId} is not sustainable or has no points", productDetail.ProductId, orderId);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogError("Failed to fetch product {ProductId} for order {OrderId}: {StatusCode}", productDetail.ProductId, orderId, productResponse.StatusCode);
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogWarning("No products found for order {OrderId}", orderId);
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to fetch products for order {OrderId}: {StatusCode}", orderId, responseProducts.StatusCode);
                        }
                    }

                    if (orders.Count > 0)
                    {
                        SustainableProductsPercent = (int)((double)sustainableProducts / allProducts * 100);
                    }
                }
                else
                {
                    _logger.LogWarning("No orders found or deserialization failed.");
                }
            }
            else
            {
                _logger.LogError("Failed to fetch orders: {StatusCode}", response.StatusCode);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _csvService.AddRecord(NewRecord);
            return RedirectToPage();
        }
    }
}
