using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ChangePasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ChangePasswordViewModel PasswordData { get; set; } = new();

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.ChangePasswordAsync(userId.Value, PasswordData);

            if (result)
            {
                TempData["SuccessMessage"] = "Hasło zostało pomyślnie zmienione!";
                return RedirectToPage("/Account/Profile");
            }

            ModelState.AddModelError("", "Aktualne hasło jest nieprawidłowe lub wystąpił błąd podczas zmiany hasła.");
            return Page();
        }
    }
}