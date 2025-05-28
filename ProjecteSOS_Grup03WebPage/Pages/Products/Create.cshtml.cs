using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjecteSOS_Grup03WebPage.DTOs;
using ProjecteSOS_Grup03WebPage.Tools;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Localization;

namespace ProjecteSOS_Grup03WebPage.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;
		private readonly IStringLocalizer<SharedResource> _localizer;

		[BindProperty]
        public ProductDTO NewProduct { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger, IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _localizer = localizer; 
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
                    return RedirectToPage("List");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = _localizer["UnauthorizedCreateProduct"];
                }
                else
                {
                    ErrorMessage = _localizer["ForbiddenCreateProduct"] + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Creating Product");
                ErrorMessage = _localizer["UnexpectedErrorTryAgain"];
            }

            return Page();
        }
    }
}
