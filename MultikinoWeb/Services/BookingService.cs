using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;
using System.Text.Json;

namespace MultikinoWeb.Services
{
    public class BookingService : IBookingService
    {
        private readonly MultikinoDbContext _context;

        public BookingService(MultikinoDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetOccupiedSeatsAsync(int screeningId)
        {
            var occupiedSeats = await _context.Tickets
                .Where(t => t.Booking.ScreeningId == screeningId &&
                           (t.Booking.Status == "Confirmed" || t.Booking.Status == "AwaitingCashPayment"))
                .Select(t => t.SeatNumber)
                .ToListAsync();

            return occupiedSeats;
        }

        public async Task<bool> CreateBookingAsync(BookingViewModel model, int userId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == model.ScreeningId);

            if (screening == null)
                return false;

            if (screening.AvailableSeats < model.NumberOfTickets)
                return false;

            if (screening.StartTime <= DateTime.Now.AddMinutes(30))
                return false;

            var selectedSeats = model.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var occupiedSeats = await GetOccupiedSeatsAsync(model.ScreeningId);

            var conflictingSeats = selectedSeats.Intersect(occupiedSeats).ToList();
            if (conflictingSeats.Any())
            {
                return false; // Wybrane miejsca są już zajęte
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    UserId = userId,
                    ScreeningId = model.ScreeningId,
                    BookingDate = DateTime.Now,
                    NumberOfTickets = model.NumberOfTickets,
                    TotalAmount = model.NumberOfTickets * screening.TicketPrice,
                    Status = "Confirmed",
                    PaymentMethod = model.PaymentMethod
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

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

        public async Task<bool> CreateCashBookingAsync(BookingViewModel model, int userId)
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == model.ScreeningId);

            if (screening == null)
                return false;

            if (screening.AvailableSeats < model.NumberOfTickets)
                return false;

            if (screening.StartTime <= DateTime.Now.AddMinutes(30))
                return false;

            var selectedSeats = model.SelectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var occupiedSeats = await GetOccupiedSeatsAsync(model.ScreeningId);

            var conflictingSeats = selectedSeats.Intersect(occupiedSeats).ToList();
            if (conflictingSeats.Any())
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    UserId = userId,
                    ScreeningId = model.ScreeningId,
                    BookingDate = DateTime.Now,
                    NumberOfTickets = model.NumberOfTickets,
                    TotalAmount = model.NumberOfTickets * screening.TicketPrice,
                    Status = "AwaitingCashPayment", // Specjalny status dla gotówki
                    PaymentMethod = "Cash"
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

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

        public async Task<int?> CreatePaidBookingAsync(string sessionData)
        {
            try
            {
                var bookingData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(sessionData);
                
                var screeningId = bookingData["ScreeningId"].GetInt32();
                var numberOfTickets = bookingData["NumberOfTickets"].GetInt32();
                var paymentMethod = bookingData["PaymentMethod"].GetString();
                var selectedSeats = bookingData["SelectedSeats"].GetString();
                var userId = bookingData["UserId"].GetInt32();

                var screening = await _context.Screenings
                    .Include(s => s.Movie)
                    .Include(s => s.Hall)
                    .FirstOrDefaultAsync(s => s.ScreeningId == screeningId);

                if (screening == null)
                    return null;

                if (screening.AvailableSeats < numberOfTickets)
                    return null;

                var selectedSeatsArray = selectedSeats.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var occupiedSeats = await GetOccupiedSeatsAsync(screeningId);
                var conflictingSeats = selectedSeatsArray.Intersect(occupiedSeats).ToList();
                
                if (conflictingSeats.Any())
                    return null;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var booking = new Booking
                    {
                        UserId = userId,
                        ScreeningId = screeningId,
                        BookingDate = DateTime.Now,
                        NumberOfTickets = numberOfTickets,
                        TotalAmount = numberOfTickets * screening.TicketPrice,
                        Status = "Confirmed", // Potwierdzona i opłacona
                        PaymentMethod = paymentMethod
                    };

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    for (int i = 0; i < numberOfTickets; i++)
                    {
                        var seatNumber = i < selectedSeatsArray.Length ? selectedSeatsArray[i] : $"AUTO{i + 1}";

                        var ticket = new Ticket
                        {
                            BookingId = booking.BookingId,
                            SeatNumber = seatNumber,
                            Price = screening.TicketPrice,
                            IsUsed = false
                        };
                        _context.Tickets.Add(ticket);
                    }

                    screening.AvailableSeats -= numberOfTickets;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return booking.BookingId;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Hall)
                .Include(b => b.Tickets)
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
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null || booking.Status == "Cancelled")
                return false;

            var timeDifference = booking.Screening.StartTime - DateTime.Now;
            if (timeDifference.TotalHours < 2)
            {
                return false; // Za późno na anulowanie
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                booking.Status = "Cancelled";

                booking.Screening.AvailableSeats += booking.NumberOfTickets;

                foreach (var ticket in booking.Tickets)
                {
                    ticket.IsUsed = true; // Można dodać pole IsCancelled zamiast używać IsUsed
                }

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

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .Include(b => b.Tickets)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Bookings.Where(b => b.Status == "Confirmed" || b.Status == "AwaitingCashPayment");

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate <= endDate.Value);

            return await query.SumAsync(b => b.TotalAmount);
        }
    }
}