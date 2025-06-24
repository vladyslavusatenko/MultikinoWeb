using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Filters;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Admin.Screenings
{
    [AdminAuthorization]
    public class AdminScreeningsIndexModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly MultikinoDbContext _context;

        public AdminScreeningsIndexModel(IAdminService adminService, MultikinoDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        public IEnumerable<Screening> Screenings { get; set; } = new List<Screening>();
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public IEnumerable<CinemaHall> Halls { get; set; } = new List<CinemaHall>();

        [BindProperty(SupportsGet = true)]
        public int? MovieFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? HallFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        public async Task OnGetAsync()
        {
            var allScreenings = await _adminService.GetAllScreeningsAsync();
            Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync();
            Halls = await _context.CinemaHalls.Where(h => h.IsActive).OrderBy(h => h.HallName).ToListAsync();

            Screenings = FilterScreenings(allScreenings);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _adminService.DeleteScreeningAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Seans został usunięty pomyślnie.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można usunąć seansu. Sprawdź czy nie ma potwierdonych rezerwacji.";
            }

            return RedirectToPage();
        }

        private IEnumerable<Screening> FilterScreenings(IEnumerable<Screening> screenings)
        {
            if (MovieFilter.HasValue)
            {
                screenings = screenings.Where(s => s.MovieId == MovieFilter.Value);
            }

            if (HallFilter.HasValue)
            {
                screenings = screenings.Where(s => s.HallId == HallFilter.Value);
            }

            if (DateFilter.HasValue)
            {
                screenings = screenings.Where(s => s.StartTime.Date == DateFilter.Value.Date);
            }

            return screenings.OrderBy(s => s.StartTime);
        }
    }
}