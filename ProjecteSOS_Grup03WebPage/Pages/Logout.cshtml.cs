using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjecteSOS_Grup03WebPage.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            HttpContext.Session.Remove("AuthToken");
            HttpContext.Session.Clear();
            return RedirectToPage("Index");
        }
    }
}
