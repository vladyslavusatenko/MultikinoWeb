using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Filters;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Reports
{
    [AdminAuthorization]
    public class AdminReportsIndexModel : PageModel
    {
        private readonly IAdminService _adminService;

        public AdminReportsIndexModel(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public RevenueReportViewModel? RevenueReport { get; set; }
        public IEnumerable<HallUtilizationViewModel> HallUtilization { get; set; } = new List<HallUtilizationViewModel>();

        public async Task OnGetAsync()
        {
            // Set default dates if not provided
            if (!StartDate.HasValue)
                StartDate = DateTime.Today.AddDays(-30);

            if (!EndDate.HasValue)
                EndDate = DateTime.Today;

            // Generate reports only if dates are provided
            if (StartDate.HasValue && EndDate.HasValue)
            {
                RevenueReport = await _adminService.GetRevenueReportAsync(StartDate, EndDate);
                HallUtilization = await _adminService.GetHallUtilizationReportAsync();
            }
        }
    }
}