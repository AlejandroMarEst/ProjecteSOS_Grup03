using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using System.Net;
using System.Text.Json;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public ProductDTO? Product { get; set; }
        public string? ErrorMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var response = await client.GetAsync($"api/Products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Product = JsonSerializer.Deserialize<ProductDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (Product == null)
                    {
                        ErrorMessage = "No s'ha trobat el producte.";
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ErrorMessage = "No s'ha trobat el producte.";
                }
                else
                {
                    _logger.LogError("Product Loading Failed");
                    ErrorMessage = "Error en carregar el producte.";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Loading Product");
                ErrorMessage = "There was an unexpected error. Try again.";
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

                var response = await client.PutAsJsonAsync($"api/Products/{id}", Product);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("List");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Has de ser administrador per a editar un producte";
                }
                else
                {
                    ErrorMessage = "Error en editar el producte: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Editing Product");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }
    }
}
