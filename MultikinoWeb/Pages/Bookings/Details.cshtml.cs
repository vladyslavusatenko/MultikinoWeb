﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Models;
using MultikinoWeb.Services;

namespace MultikinoWeb.Pages.Bookings
{
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public DetailsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public Booking? Booking { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Booking = await _bookingService.GetBookingByIdAsync(id);

            if (Booking == null || (Booking.UserId != userId && HttpContext.Session.GetString("UserRole") != "Admin"))
            {
                TempData["ErrorMessage"] = "Rezerwacja nie została znaleziona lub nie masz do niej dostępu.";
                return RedirectToPage("/Bookings/MyBookings");
            }

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
                TempData["SuccessMessage"] = "Rezerwacja została anulowana pomyślnie.";
            }
            else
            {
                TempData["ErrorMessage"] = "Nie można anulować tej rezerwacji. Sprawdź czy anulowanie jest możliwe do 2h przed seansem.";
            }

            return RedirectToPage("/Bookings/MyBookings");
        }
    }
}