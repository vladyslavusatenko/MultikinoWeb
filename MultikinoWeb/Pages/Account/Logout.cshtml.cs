using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MultikinoWeb.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Zostałeś pomyślnie wylogowany.";
            return RedirectToPage("/Index");
        }
    }
}