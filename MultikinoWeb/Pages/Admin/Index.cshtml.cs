using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin
{
    [AdminAuthorization]
    public class AdminIndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public AdminIndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public AdminDashboardViewModel Dashboard { get; set; } = new();

        public async Task OnGetAsync()
        {
            Dashboard = await _adminService.GetDashboardDataAsync();
        }
    }
}