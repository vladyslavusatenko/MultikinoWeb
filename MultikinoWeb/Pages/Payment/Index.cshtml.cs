using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Pages.Payment
{
    public class PaymentIndexModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public PaymentIndexModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [BindProperty]
        public PaymentDetailsViewModel Payment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            Payment.BookingId = bookingId;
            Payment.Amount = booking.TotalAmount;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Symulacja płatności
            await Task.Delay(2000); // Symuluj czas przetwarzania

            TempData["SuccessMessage"] = "Płatność została przetworzona pomyślnie!";
            return RedirectToPage("/Bookings/MyBookings");
        }
    }
}