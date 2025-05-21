using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public ProductDTO NewProduct { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("SosApi");
                var token = HttpContext.Session.GetString("AuthToken");

                if (TokenHelper.IsTokenSession(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.PostAsJsonAsync("api/Products/Add", NewProduct);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Index");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Has de ser administrador per a afegir productes";
                    return Page();
                }
                else
                {
                    ErrorMessage = "Error en crear el producte: " + response.StatusCode;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Creating Product");
                ErrorMessage = "There was an unexpected error. Try again.";
            }

            return Page();
        }
    }
}
