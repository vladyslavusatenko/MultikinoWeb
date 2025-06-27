using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultikinoWeb.Services;
using MultikinoWeb.ViewModels;
using System.Text.Json;

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

        public async Task<IActionResult> OnGetAsync(int? bookingId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var pendingBookingJson = HttpContext.Session.GetString("PendingBooking");
            Console.WriteLine($"=== PAYMENT DEBUG ===");
            Console.WriteLine($"BookingId parameter: {bookingId}");
            Console.WriteLine($"Session data: {pendingBookingJson}");
            Console.WriteLine($"Session data is null/empty: {string.IsNullOrEmpty(pendingBookingJson)}");

            if (bookingId.HasValue)
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId.Value);
                if (booking == null || booking.UserId != userId)
                {
                    return NotFound();
                }

                Payment.BookingId = bookingId.Value;
                Payment.Amount = booking.TotalAmount;
                Payment.PaymentMethod = booking.PaymentMethod;

                ViewData["MovieTitle"] = booking.Screening.Movie.Title;
                ViewData["HallName"] = booking.Screening.Hall.HallName;
                ViewData["NumberOfTickets"] = booking.NumberOfTickets.ToString();
                ViewData["StartTime"] = booking.Screening.StartTime.ToString();

                return Page();
            }

            if (string.IsNullOrEmpty(pendingBookingJson))
            {
                Console.WriteLine("ERROR: No session data found");
                TempData["ErrorMessage"] = "Nie znaleziono danych rezerwacji. Rozpocznij proces od nowa.";
                return RedirectToPage("/Movies/Index");
            }

            try
            {
                Console.WriteLine("Trying to deserialize session data...");
                var bookingData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(pendingBookingJson);

                Payment.Amount = bookingData["TotalAmount"].GetDecimal();
                Payment.PaymentMethod = bookingData["PaymentMethod"].GetString();

                ViewData["MovieTitle"] = bookingData["MovieTitle"].GetString();
                ViewData["HallName"] = bookingData["HallName"].GetString();
                ViewData["NumberOfTickets"] = bookingData["NumberOfTickets"].ToString();
                ViewData["StartTime"] = bookingData["StartTime"].GetString();
                ViewData["IsFromSession"] = true;

                Console.WriteLine($"Successfully loaded session data for {ViewData["MovieTitle"]}");
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR deserializing: {ex.Message}");
                TempData["ErrorMessage"] = "Błąd w danych rezerwacji. Rozpocznij proces od nowa.";
                return RedirectToPage("/Movies/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var pendingBookingJson = HttpContext.Session.GetString("PendingBooking");

            if (!string.IsNullOrEmpty(pendingBookingJson))
            {
                try
                {
                    if (string.IsNullOrEmpty(Payment.PaymentMethod))
                    {
                        ModelState.AddModelError("", "Proszę wybrać metodę płatności.");
                        return Page();
                    }

                    var validationError = ValidatePaymentSimple();
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        ModelState.AddModelError("", validationError);
                        return Page();
                    }

                    await Task.Delay(2000);

                    if (new Random().NextDouble() < 0.10)
                    {
                        var errors = Payment.PaymentMethod switch
                        {
                            "Card" => new[] {
                                "Karta została odrzucona przez bank.",
                                "Brak wystarczających środków na koncie.",
                                "Przekroczono dzienny limit transakcji."
                            },
                            "BLIK" => new[] {
                                "Kod BLIK wygasł lub jest nieprawidłowy.",
                                "Nie potwierdzono transakcji w aplikacji bankowej.",
                                "Brak wystarczających środków na koncie."
                            },
                            "Transfer" => new[] {
                                "Sesja bankowa została przerwana.",
                                "Nie udało się połączyć z bankiem.",
                                "Brak autoryzacji do wykonania przelewu."
                            },
                            _ => new[] { "Wystąpił błąd podczas płatności." }
                        };

                        var randomError = errors[new Random().Next(errors.Length)];
                        ModelState.AddModelError("", randomError);
                        return Page();
                    }

                    var bookingId = await _bookingService.CreatePaidBookingAsync(pendingBookingJson);

                    if (bookingId.HasValue)
                    {
                        HttpContext.Session.Remove("PendingBooking");

                        string message = Payment.PaymentMethod switch
                        {
                            "Card" => "Płatność kartą została przetworzona pomyślnie!",
                            "BLIK" => "Płatność BLIK została zatwierdzona!",
                            "Transfer" => "Przelew bankowy został zrealizowany!",
                            _ => "Płatność została przetworzona pomyślnie!"
                        };

                        TempData["SuccessMessage"] = message;
                        return RedirectToPage("/Bookings/Details", new { id = bookingId.Value });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Nie udało się utworzyć rezerwacji. Wybrane miejsca mogły zostać zajęte.";
                        return RedirectToPage("/Movies/Index");
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Wystąpił błąd podczas przetwarzania płatności.";
                    return Page();
                }
            }
            else
            {
                await Task.Delay(2000);
                TempData["SuccessMessage"] = "Płatność została przetworzona pomyślnie!";
                return RedirectToPage("/Bookings/MyBookings");
            }
        }

        private string ValidatePaymentSimple()
        {
            if (new Random().NextDouble() < 0.05)
            {
                return Payment.PaymentMethod switch
                {
                    "Card" => "Dane karty są nieprawidłowe. Sprawdź wprowadzone informacje.",
                    "BLIK" => "Nieprawidłowy kod BLIK lub numer telefonu.",
                    "Transfer" => "Wybrany bank jest obecnie niedostępny.",
                    _ => "Błąd walidacji danych płatności."
                };
            }

            return string.Empty; // Brak błędów
        }
    }
}