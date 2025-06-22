using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userService.AuthenticateAsync(LoginData.Email, LoginData.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Nieprawidłowy email lub hasło.");
                return Page();
            }

            // Ustawienie sesji
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["SuccessMessage"] = "Pomyślnie zalogowano!";

            // Przekierowanie w zależności od roli
            if (user.Role == "Admin")
            {
                return RedirectToPage("/Admin/Index");
            }

            return RedirectToPage("/Index");
        }
    }
}