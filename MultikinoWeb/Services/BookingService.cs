using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public class BookingService : IBookingService
    {
        private readonly MultikinoDbContext _context;

        public BookingService(MultikinoDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateBookingAsync(BookingViewModel model, int userId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == model.ScreeningId);

            if (screening == null)
                return false;

            // SPRAWDŹ DOSTĘPNOŚĆ MIEJSC
            if (screening.AvailableSeats < model.NumberOfTickets)
                return false;

            // SPRAWDŹ CZY SEANS JESZCZE SIĘ NIE ROZPOCZĄŁ
            if (screening.StartTime <= DateTime.Now)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    UserId = userId,
                    ScreeningId = model.ScreeningId,
                    BookingDate = DateTime.Now,
                    NumberOfTickets = model.NumberOfTickets,
                    TotalAmount = model.NumberOfTickets * screening.TicketPrice, // Użyj ceny z bazy
                    Status = "Confirmed",
                    PaymentMethod = model.PaymentMethod
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // UTWÓRZ BILETY Z WYBRANYMI MIEJSCAMI
                var selectedSeats = model.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < model.NumberOfTickets; i++)
                {
                    var seatNumber = i < selectedSeats.Length ? selectedSeats[i] : $"AUTO{i + 1}";

                    var ticket = new Ticket
                    {
                        BookingId = booking.BookingId,
                        SeatNumber = seatNumber,
                        Price = screening.TicketPrice,
                        IsUsed = false
                    };
                    _context.Tickets.Add(ticket);
                }

                // ZAKTUALIZUJ DOSTĘPNE MIEJSCA
                screening.AvailableSeats -= model.NumberOfTickets;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Hall)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Hall)
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<bool> CancelBookingAsync(int id, int userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null || booking.Status == "Cancelled")
                return false;

            // SPRAWDŹ CZY MOŻNA ANULOWAĆ (2H PRZED SEANSEM)
            var timeDifference = booking.Screening.StartTime - DateTime.Now;
            if (timeDifference.TotalHours < 2)
            {
                return false; // Za późno na anulowanie
            }

            booking.Status = "Cancelled";
            booking.Screening.AvailableSeats += booking.NumberOfTickets;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Bookings.Where(b => b.Status == "Confirmed");

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate <= endDate.Value);

            return await query.SumAsync(b => b.TotalAmount);
        }

    }

}