using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public interface IBookingService
    {
        Task<bool> CreateBookingAsync(BookingViewModel model, int userId);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<bool> CancelBookingAsync(int id, int userId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);

        // NOWA METODA - Pobieranie zajętych miejsc
        Task<List<string>> GetOccupiedSeatsAsync(int screeningId);
    }
}