using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Users
{
    [AdminAuthorization]
    public class AdminUsersIndexModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;

        public AdminUsersIndexModel(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public IEnumerable<User> Users { get; set; } = new List<User>();
        public AdminUsersOverviewViewModel Overview { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            Overview = await _userManagementService.GetUsersOverviewAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Users = await _userManagementService.SearchUsersAsync(SearchTerm);
            }
            else
            {
                Users = await _userManagementService.GetAllUsersWithStatsAsync();
            }

            Users = ApplyFilters(Users);
        }

        public async Task<IActionResult> OnPostBanAsync(int id)
        {
            var result = await _userManagementService.BanUserAsync(id, "Zablokowany przez administratora");

            if (result)
            {
                TempData["SuccessMessage"] = "Użytkownik został zablokowany.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można zablokować tego użytkownika.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanAsync(int id)
        {
            var result = await _userManagementService.UnbanUserAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Użytkownik został odblokowany.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można odblokować tego użytkownika.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(int id, string role)
        {
            var result = await _userManagementService.ChangeUserRoleAsync(id, role);

            if (result)
            {
                TempData["SuccessMessage"] = $"Rola użytkownika została zmieniona na {role}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można zmienić roli użytkownika.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _userManagementService.DeleteUserAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Użytkownik został usunięty.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można usunąć użytkownika (ma aktywne rezerwacje lub jest administratorem).";
            }

            return RedirectToPage();
        }

        private IEnumerable<User> ApplyFilters(IEnumerable<User> users)
        {
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                bool isActive = bool.Parse(StatusFilter);
                users = users.Where(u => u.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(RoleFilter))
            {
                users = users.Where(u => u.Role == RoleFilter);
            }

            return users.OrderByDescending(u => u.CreatedAt);
        }
    }
}