using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Bookings
{
    public class MyBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public MyBookingsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public IEnumerable<Booking> Bookings { get; set; } = new List<Booking>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Bookings = await _bookingService.GetUserBookingsAsync(userId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var result = await _bookingService.CancelBookingAsync(id, userId.Value);

            if (result)
            {
                TempData["SuccessMessage"] = "Rezerwacja została anulowana.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można anulować tej rezerwacji.";
            }

            return RedirectToPage();
        }
    }
}