using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;

namespace MultikinoWeb.Services
{
    public class ScreeningCleanupService : IScreeningCleanupService
    {
        private readonly MultikinoDbContext _context;
        private readonly ILogger<ScreeningCleanupService> _logger;

        public ScreeningCleanupService(MultikinoDbContext context, ILogger<ScreeningCleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessExpiredScreeningsAsync()
        {
            try
            {
                var expiredScreenings = await _context.Screenings
                    .Include(s => s.Bookings)
                    .ThenInclude(b => b.Tickets)
                    .Where(s => s.EndTime <= DateTime.Now &&
                                s.Bookings.Any(b => b.Status == "Confirmed"))
                    .ToListAsync();

                foreach (var screening in expiredScreenings)
                {
                    foreach (var booking in screening.Bookings.Where(b => b.Status == "Confirmed"))
                    {
                        booking.Status = "Completed"; // Nowy status

                        foreach (var ticket in booking.Tickets)
                        {
                            ticket.IsUsed = true;
                        }
                    }
                }

                if (expiredScreenings.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Przetworzono {expiredScreenings.Count} zakończonych seansów.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas przetwarzania zakończonych seansów.");
            }
        }
    }
}