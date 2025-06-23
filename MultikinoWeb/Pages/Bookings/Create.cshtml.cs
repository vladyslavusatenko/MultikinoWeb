using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Bookings
{
    public class CreateBookingModel : PageModel
    {
        private readonly MultikinoDbContext _context;
        private readonly IBookingService _bookingService;

        public CreateBookingModel(MultikinoDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        [BindProperty]
        public BookingViewModel BookingData { get; set; } = new();

        public Screening? Screening { get; set; }

        // NOWA WŁAŚCIWOŚĆ - Lista zajętych miejsc
        public List<string> OccupiedSeats { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int screeningId)
        {
            // WYCZYŚĆ EWENTUALNE STARE DANE Z SESJI
            HttpContext.Session.Remove("PendingBooking");

            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == screeningId);

            if (Screening == null)
            {
                return NotFound();
            }

            // SPRAWDŹ CZY SEANS JESZCZE SIĘ NIE ROZPOCZĄŁ
            if (Screening.StartTime <= DateTime.Now.AddMinutes(30)) // 30 min przed rozpoczęciem
            {
                TempData["ErrorMessage"] = "Rezerwacja nie jest już możliwa - seans rozpoczyna się za mniej niż 30 minut.";
                return RedirectToPage("/Movies/Details", new { id = Screening.MovieId });
            }

            // POBIERZ RZECZYWISTE ZAJĘTE MIEJSCA Z BAZY DANYCH
            OccupiedSeats = await _bookingService.GetOccupiedSeatsAsync(screeningId);

            BookingData.ScreeningId = screeningId;
            BookingData.TicketPrice = Screening.TicketPrice;
            BookingData.NumberOfTickets = 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Pobierz ponownie screening i zajęte miejsca
            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == BookingData.ScreeningId);

            if (Screening == null)
            {
                return NotFound();
            }

            // Pobierz zajęte miejsca
            OccupiedSeats = await _bookingService.GetOccupiedSeatsAsync(BookingData.ScreeningId);

            // Sprawdź czy użytkownik jest zalogowany
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Musisz być zalogowany, aby dokonać rezerwacji.";
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Sprawdź dostępność miejsc
            if (Screening.AvailableSeats < BookingData.NumberOfTickets)
            {
                ModelState.AddModelError("", "Brak wystarczającej liczby dostępnych miejsc.");
                return Page();
            }

            // Sprawdź czy wybrane miejsca nie są już zajęte
            var selectedSeats = BookingData.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var conflictingSeats = selectedSeats.Intersect(OccupiedSeats).ToList();

            if (conflictingSeats.Any())
            {
                ModelState.AddModelError("", $"Wybrane miejsca są już zajęte: {string.Join(", ", conflictingSeats)}");
                return Page();
            }

            var result = await _bookingService.CreateBookingAsync(BookingData, userId.Value);

            if (result)
            {
                TempData["SuccessMessage"] = "Rezerwacja została utworzona pomyślnie!";
                return RedirectToPage("/Bookings/MyBookings");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia rezerwacji. Wybrane miejsca mogą być już zajęte.");
            return Page();
        }

        // NOWA METODA API - Pobieranie zajętych miejsc przez AJAX
        public async Task<IActionResult> OnGetOccupiedSeatsAsync(int screeningId)
        {
            var occupiedSeats = await _bookingService.GetOccupiedSeatsAsync(screeningId);
            return new JsonResult(occupiedSeats);
        }
    }
}