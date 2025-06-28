using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Admin.Screenings
{
    [AdminAuthorization]
    public class AdminScreeningsEditModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly MultikinoDbContext _context;

        public AdminScreeningsEditModel(IAdminService adminService, MultikinoDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        [BindProperty]
        public EditScreeningViewModel ScreeningData { get; set; } = new();

        public Screening Screening { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.ScreeningId == id);

            if (Screening == null)
            {
                TempData["ErrorMessage"] = "Seans nie został znaleziony.";
                return RedirectToPage("/Admin/Screenings/Index");
            }

            // Check if screening can be edited (hasn't started yet)
            if (Screening.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Nie można edytować seansu, który już się rozpoczął.";
                return RedirectToPage("/Admin/Screenings/Index");
            }

            // Check if there are any confirmed bookings
            if (Screening.Bookings.Any(b => b.Status == "Confirmed"))
            {
                TempData["WarningMessage"] = "Uwaga: Ten seans ma już potwierdzone rezerwacje. Zmiany mogą wpłynąć na klientów.";
            }

            await LoadDropdownDataAsync();
            PopulateScreeningData();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadDropdownDataAsync();

            if (!ModelState.IsValid)
            {
                Screening = await _context.Screenings
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .FirstOrDefaultAsync(s => s.ScreeningId == id);
                return Page();
            }

            var result = await _adminService.UpdateScreeningAsync(id, ScreeningData);

            if (result)
            {
                TempData["SuccessMessage"] = "Seans został zaktualizowany pomyślnie!";
                return RedirectToPage("/Admin/Screenings/Index");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas aktualizacji seansu.");
            return Page();
        }

        private async Task LoadDropdownDataAsync()
        {
            ScreeningData.Movies = await _context.Movies
                .Where(m => m.IsActive)
                .OrderBy(m => m.Title)
                .ToListAsync();

            ScreeningData.Halls = await _context.CinemaHalls
                .Where(h => h.IsActive)
                .OrderBy(h => h.HallName)
                .ToListAsync();
        }

        private void PopulateScreeningData()
        {
            ScreeningData.MovieId = Screening.MovieId;
            ScreeningData.HallId = Screening.HallId;
            ScreeningData.StartTime = Screening.StartTime;
            ScreeningData.TicketPrice = Screening.TicketPrice;
        }
    }
}