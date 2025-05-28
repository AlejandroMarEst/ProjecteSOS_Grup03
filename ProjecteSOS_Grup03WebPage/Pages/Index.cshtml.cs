using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public List<ProductListDTO> Products { get; set; } = new List<ProductListDTO>();
        public string? ErrorMessage { get; set; }
        public bool IsEmployee { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRoles = TokenHelper.GetUserRoles(token).ToList();
            IsEmployee = userRoles.Contains("Employee") || userRoles.Contains("Admin");

            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var response = await client.GetAsync("api/Products");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductListDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (products != null)
                    {
                        Products = products.OrderByDescending(products => products.Price).ToList();
                    }
                    else
                    {
                        Products = new List<ProductListDTO>();
                    }
                }
                else
                {
                    _logger.LogError("Failed to load products for Index page. Status {StatusCode}", response.StatusCode);

                    ErrorMessage = "Error loading products.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products for Index page.");

                ErrorMessage = "An unexpected error occurred.";
            }
        }

        public IActionResult OnPostSetCultureAsync(string culture, string returnUrl)
        {
            // Guardar la preferència d'idioma de l'usuari en una cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });

            _logger.LogInformation("Culture cookie should be set. Redirecting to: {ReturnUrl}", returnUrl);

            return LocalRedirect(returnUrl);
        }
    }
}


