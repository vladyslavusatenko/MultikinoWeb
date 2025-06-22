using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;

        public RegisterModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RegisterViewModel RegisterData { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _userService.RegisterAsync(RegisterData);

            if (!result)
            {
                ModelState.AddModelError("", "Użytkownik z tym adresem email już istnieje.");
                return Page();
            }

            TempData["SuccessMessage"] = "Rejestracja przebiegła pomyślnie! Możesz się teraz zalogować.";
            return RedirectToPage("/Account/Login");
        }
    }
}