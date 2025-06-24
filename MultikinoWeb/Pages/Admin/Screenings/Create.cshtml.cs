using System;
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
    public class AdminScreeningsCreateModel : PageModel
    {
        private readonly IAdminService _adminService;
        private readonly MultikinoDbContext _context;

        public AdminScreeningsCreateModel(IAdminService adminService, MultikinoDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        [BindProperty]
        public CreateScreeningViewModel ScreeningData { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDropdownDataAsync();

            // Set default values
            ScreeningData.StartTime = DateTime.Now.AddHours(1).Date.AddHours(DateTime.Now.Hour + 1);
            ScreeningData.TicketPrice = 25.00m;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDropdownDataAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Additional validations
            if (ScreeningData.StartTime <= DateTime.Now.AddMinutes(30))
            {
                ModelState.AddModelError("ScreeningData.StartTime", "Seans musi rozpocząć się przynajmniej 30 minut od teraz.");
                return Page();
            }

            var movie = await _context.Movies.FindAsync(ScreeningData.MovieId);
            var hall = await _context.CinemaHalls.FindAsync(ScreeningData.HallId);

            if (movie == null || hall == null)
            {
                ModelState.AddModelError("", "Wybrany film lub sala nie istnieje.");
                return Page();
            }

            // Check if movie is active
            if (!movie.IsActive)
            {
                ModelState.AddModelError("ScreeningData.MovieId", "Wybrany film nie jest aktywny.");
                return Page();
            }

            // Check if hall is active
            if (!hall.IsActive)
            {
                ModelState.AddModelError("ScreeningData.HallId", "Wybrana sala nie jest aktywna.");
                return Page();
            }

            // Calculate end time
            var endTime = ScreeningData.StartTime.AddMinutes(movie.Duration + 30);

            // Check hall availability
            if (!await _adminService.IsHallAvailableAsync(ScreeningData.HallId, ScreeningData.StartTime, endTime))
            {
                ModelState.AddModelError("", "Sala jest zajęta w wybranym terminie. Sprawdź dostępność i wybierz inny termin.");
                return Page();
            }

            // Create screening
            var result = await _adminService.CreateScreeningAsync(ScreeningData);

            if (result)
            {
                TempData["SuccessMessage"] = $"Seans filmu '{movie.Title}' został utworzony pomyślnie!";
                return RedirectToPage("/Admin/Screenings/Index");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia seansu.");
            return Page();
        }

        // AJAX endpoint for checking availability
        public async Task<IActionResult> OnGetCheckAvailabilityAsync(int movieId, int hallId, DateTime startTime)
        {
            try
            {
                // Sprawdź czy parametry są prawidłowe
                if (movieId <= 0 || hallId <= 0)
                {
                    return new JsonResult(new { available = false, message = "Nieprawidłowe dane wejściowe." });
                }

                // Sprawdź czy data nie jest w przeszłości
                if (startTime <= DateTime.Now.AddMinutes(30))
                {
                    return new JsonResult(new { available = false, message = "Seans musi rozpocząć się przynajmniej 30 minut od teraz." });
                }

                // Pobierz film i salę
                var movie = await _context.Movies.FindAsync(movieId);
                var hall = await _context.CinemaHalls.FindAsync(hallId);

                if (movie == null)
                {
                    return new JsonResult(new { available = false, message = "Wybrany film nie istnieje." });
                }

                if (hall == null)
                {
                    return new JsonResult(new { available = false, message = "Wybrana sala nie istnieje." });
                }

                if (!movie.IsActive)
                {
                    return new JsonResult(new { available = false, message = "Wybrany film nie jest aktywny." });
                }

                if (!hall.IsActive)
                {
                    return new JsonResult(new { available = false, message = "Wybrana sala nie jest aktywna." });
                }

                // Oblicz czas zakończenia
                var endTime = startTime.AddMinutes(movie.Duration + 30);

                // Sprawdź dostępność sali
                var isAvailable = await _adminService.IsHallAvailableAsync(hallId, startTime, endTime);

                if (isAvailable)
                {
                    return new JsonResult(new
                    {
                        available = true,
                        message = $"Sala '{hall.HallName}' jest dostępna dla filmu '{movie.Title}' w terminie {startTime:dd.MM.yyyy HH:mm} - {endTime:HH:mm}"
                    });
                }
                else
                {
                    // Znajdź konfliktujący seans
                    var conflictingScreening = await _context.Screenings
                        .Include(s => s.Movie)
                        .Where(s => s.HallId == hallId)
                        .Where(s =>
                            (startTime >= s.StartTime && startTime < s.EndTime) ||
                            (endTime > s.StartTime && endTime <= s.EndTime) ||
                            (startTime <= s.StartTime && endTime >= s.EndTime))
                        .FirstOrDefaultAsync();

                    var message = conflictingScreening != null
                        ? $"Konflikt z seansem filmu '{conflictingScreening.Movie.Title}' ({conflictingScreening.StartTime:dd.MM HH:mm} - {conflictingScreening.EndTime:HH:mm})"
                        : "Sala jest zajęta w tym terminie.";

                    return new JsonResult(new { available = false, message });
                }
            }
            catch (Exception ex)
            {
                // Loguj błąd w rzeczywistej aplikacji
                return new JsonResult(new { available = false, message = "Wystąpił błąd podczas sprawdzania dostępności. Spróbuj ponownie." });
            }
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
    }
}
