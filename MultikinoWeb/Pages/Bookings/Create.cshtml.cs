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

        public List<string> OccupiedSeats { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int screeningId)
        {
            HttpContext.Session.Remove("PendingBooking");

            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == screeningId);

            if (Screening == null)
            {
                return NotFound();
            }

            if (Screening.StartTime <= DateTime.Now.AddMinutes(30)) // 30 min przed rozpoczęciem
            {
                TempData["ErrorMessage"] = "Rezerwacja nie jest już możliwa - seans rozpoczyna się za mniej niż 30 minut.";
                return RedirectToPage("/Movies/Details", new { id = Screening.MovieId });
            }

            OccupiedSeats = await _bookingService.GetOccupiedSeatsAsync(screeningId);

            BookingData.ScreeningId = screeningId;
            BookingData.TicketPrice = Screening.TicketPrice;
            BookingData.NumberOfTickets = 1;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == BookingData.ScreeningId);

            if (Screening == null)
            {
                return NotFound();
            }

            OccupiedSeats = await _bookingService.GetOccupiedSeatsAsync(BookingData.ScreeningId);

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

            if (Screening.AvailableSeats < BookingData.NumberOfTickets)
            {
                ModelState.AddModelError("", "Brak wystarczającej liczby dostępnych miejsc.");
                return Page();
            }

            var selectedSeats = BookingData.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var conflictingSeats = selectedSeats.Intersect(OccupiedSeats).ToList();

            if (conflictingSeats.Any())
            {
                ModelState.AddModelError("", $"Wybrane miejsca są już zajęte: {string.Join(", ", conflictingSeats)}");
                return Page();
            }

            if (BookingData.PaymentMethod == "Cash")
            {
                var result = await _bookingService.CreateCashBookingAsync(BookingData, userId.Value);

                if (result)
                {
                    TempData["SuccessMessage"] = "Rezerwacja została utworzona! Zapłać gotówką w kasie przed seansem.";
                    return RedirectToPage("/Bookings/MyBookings");
                }
                else
                {
                    ModelState.AddModelError("", "Wystąpił błąd podczas tworzenia rezerwacji.");
                    return Page();
                }
            }
            else
            {
                try
                {
                    var bookingSession = new
                    {
                        ScreeningId = BookingData.ScreeningId,
                        NumberOfTickets = BookingData.NumberOfTickets,
                        PaymentMethod = BookingData.PaymentMethod,
                        SelectedSeats = BookingData.SelectedSeats,
                        TotalAmount = BookingData.NumberOfTickets * Screening.TicketPrice,
                        UserId = userId.Value,
                        MovieTitle = Screening.Movie.Title,
                        HallName = Screening.Hall.HallName,
                        StartTime = Screening.StartTime.ToString("yyyy-MM-dd HH:mm:ss") // Sformatuj datę jako string
                    };

                    var jsonString = System.Text.Json.JsonSerializer.Serialize(bookingSession);
                    HttpContext.Session.SetString("PendingBooking", jsonString);

                    TempData["InfoMessage"] = "Przejdź do płatności aby potwierdzić rezerwację.";
                    return RedirectToPage("/Payment/Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Błąd podczas przygotowywania płatności: " + ex.Message);
                    return Page();
                }
            }
        }

        public async Task<IActionResult> OnGetOccupiedSeatsAsync(int screeningId)
        {
            var occupiedSeats = await _bookingService.GetOccupiedSeatsAsync(screeningId);
            return new JsonResult(occupiedSeats);
        }
    }
}