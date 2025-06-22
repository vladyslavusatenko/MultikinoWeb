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

            BookingData.ScreeningId = screeningId;
            BookingData.TicketPrice = Screening.TicketPrice;
            BookingData.NumberOfTickets = 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Pobierz ponownie screening
            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == BookingData.ScreeningId);

            if (Screening == null)
            {
                return NotFound();
            }

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

            var result = await _bookingService.CreateBookingAsync(BookingData, userId.Value);

            if (result)
            {
                TempData["SuccessMessage"] = "Rezerwacja została utworzona pomyślnie!";
                return RedirectToPage("/Bookings/MyBookings");
            }

            ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia rezerwacji.");
            return Page();
        }
    }
}