using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Users
{
    [AdminAuthorization]
    public class UserDetailsModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;

        public UserDetailsModel(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public UserStatisticsViewModel UserStats { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            UserStats = await _userManagementService.GetUserStatisticsAsync(id);

            if (UserStats == null)
            {
                TempData["ErrorMessage"] = "Użytkownik nie został znaleziony.";
                return RedirectToPage("/Admin/Users/Index");
            }

            return Page();
        }
    }
}