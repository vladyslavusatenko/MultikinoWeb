using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public class AdminService : IAdminService
    {
        private readonly MultikinoDbContext _context;

        public AdminService(MultikinoDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var dashboard = new AdminDashboardViewModel
            {
                TotalMovies = await _context.Movies.CountAsync(m => m.IsActive),
                TotalHalls = await _context.CinemaHalls.CountAsync(h => h.IsActive),
                TotalUsers = await _context.Users.CountAsync(u => u.IsActive),

                TodayRevenue = await _context.Bookings
                    .Where(b => b.BookingDate.Date == today && b.Status == "Confirmed")
                    .SumAsync(b => b.TotalAmount),

                ThisMonthRevenue = await _context.Bookings
                    .Where(b => b.BookingDate >= thisMonth && b.Status == "Confirmed")
                    .SumAsync(b => b.TotalAmount),

                LastMonthRevenue = await _context.Bookings
                    .Where(b => b.BookingDate >= lastMonth && b.BookingDate < thisMonth && b.Status == "Confirmed")
                    .SumAsync(b => b.TotalAmount),

                TodayBookings = await _context.Bookings
                    .CountAsync(b => b.BookingDate.Date == today),

                PendingBookings = await _context.Bookings
                    .CountAsync(b => b.Status == "Pending"),

                TodayScreenings = await _context.Screenings
                    .CountAsync(s => s.StartTime.Date == today),

                UpcomingScreenings = await _context.Screenings
                    .CountAsync(s => s.StartTime > DateTime.Now)
            };

            // Recent bookings
            dashboard.RecentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .ToListAsync();

            // Popular movies
            dashboard.PopularMovies = await _context.Bookings
                .Where(b => b.BookingDate >= thisMonth && b.Status == "Confirmed")
                .GroupBy(b => b.Screening.Movie)
                .Select(g => new PopularMovieViewModel
                {
                    Movie = g.Key,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(m => m.TotalBookings)
                .Take(5)
                .ToListAsync();

            return dashboard;
        }

        public async Task<IEnumerable<CinemaHall>> GetAllHallsAsync()
        {
            return await _context.CinemaHalls.OrderBy(h => h.HallName).ToListAsync();
        }

        public async Task<CinemaHall?> GetHallByIdAsync(int id)
        {
            return await _context.CinemaHalls.FindAsync(id);
        }

        public async Task<bool> CreateHallAsync(CreateHallViewModel model)
        {
            if (await _context.CinemaHalls.AnyAsync(h => h.HallName == model.HallName))
                return false;

            var hall = new CinemaHall
            {
                HallName = model.HallName,
                Capacity = model.Capacity,
                HallType = model.HallType,
                IsActive = true
            };

            _context.CinemaHalls.Add(hall);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateHallAsync(CinemaHall hall)
        {
            _context.CinemaHalls.Update(hall);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteHallAsync(int id)
        {
            var hall = await _context.CinemaHalls.FindAsync(id);
            if (hall != null)
            {
                // Check if hall has future screenings
                var hasFutureScreenings = await _context.Screenings
                    .AnyAsync(s => s.HallId == id && s.StartTime > DateTime.Now);

                if (hasFutureScreenings)
                    return false;

                hall.IsActive = false;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<IEnumerable<Screening>> GetAllScreeningsAsync()
        {
            return await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Screening?> GetScreeningByIdAsync(int id)
        {
            return await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.ScreeningId == id);
        }

        public async Task<bool> CreateScreeningAsync(CreateScreeningViewModel model)
        {
            var movie = await _context.Movies.FindAsync(model.MovieId);
            var hall = await _context.CinemaHalls.FindAsync(model.HallId);

            if (movie == null || hall == null)
                return false;

            var endTime = model.StartTime.AddMinutes(movie.Duration + 30); // 30 min break

            // Check hall availability
            if (!await IsHallAvailableAsync(model.HallId, model.StartTime, endTime))
                return false;

            var screening = new Screening
            {
                MovieId = model.MovieId,
                HallId = model.HallId,
                StartTime = model.StartTime,
                EndTime = endTime,
                TicketPrice = model.TicketPrice,
                AvailableSeats = hall.Capacity
            };

            _context.Screenings.Add(screening);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateScreeningAsync(Screening screening)
        {
            var movie = await _context.Movies.FindAsync(screening.MovieId);
            if (movie != null)
            {
                screening.EndTime = screening.StartTime.AddMinutes(movie.Duration + 30);
            }

            _context.Screenings.Update(screening);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteScreeningAsync(int id)
        {
            var screening = await _context.Screenings
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.ScreeningId == id);

            if (screening != null)
            {
                // Check if screening has confirmed bookings
                var hasConfirmedBookings = screening.Bookings.Any(b => b.Status == "Confirmed");
                if (hasConfirmedBookings)
                    return false;

                _context.Screenings.Remove(screening);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> IsHallAvailableAsync(int hallId, DateTime startTime, DateTime endTime, int? excludeScreeningId = null)
        {
            var query = _context.Screenings.Where(s => s.HallId == hallId);

            if (excludeScreeningId.HasValue)
                query = query.Where(s => s.ScreeningId != excludeScreeningId.Value);

            return !await query.AnyAsync(s =>
                (startTime >= s.StartTime && startTime < s.EndTime) ||
                (endTime > s.StartTime && endTime <= s.EndTime) ||
                (startTime <= s.StartTime && endTime >= s.EndTime));
        }

        public async Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Bookings.Where(b => b.Status == "Confirmed");

            if (startDate.HasValue)
                query = query.Where(b => b.BookingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.BookingDate <= endDate.Value);

            var bookings = await query
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                .ThenInclude(s => s.Hall)
                .ToListAsync();

            return new RevenueReportViewModel
            {
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MaxValue,
                TotalRevenue = bookings.Sum(b => b.TotalAmount),
                TotalBookings = bookings.Count,
                TotalTickets = bookings.Sum(b => b.NumberOfTickets),
                AverageTicketPrice = bookings.Any() ? bookings.Average(b => b.TotalAmount / b.NumberOfTickets) : 0,

                RevenueByMovie = bookings
                    .GroupBy(b => b.Screening.Movie)
                    .Select(g => new MovieRevenueViewModel
                    {
                        Movie = g.Key,
                        Revenue = g.Sum(b => b.TotalAmount),
                        Bookings = g.Count(),
                        Tickets = g.Sum(b => b.NumberOfTickets)
                    })
                    .OrderByDescending(m => m.Revenue)
                    .ToList(),

                RevenueByHall = bookings
                    .GroupBy(b => b.Screening.Hall)
                    .Select(g => new HallRevenueViewModel
                    {
                        Hall = g.Key,
                        Revenue = g.Sum(b => b.TotalAmount),
                        Bookings = g.Count(),
                        Tickets = g.Sum(b => b.NumberOfTickets)
                    })
                    .OrderByDescending(h => h.Revenue)
                    .ToList(),

                DailyRevenue = bookings
                    .GroupBy(b => b.BookingDate.Date)
                    .Select(g => new DailyRevenueViewModel
                    {
                        Date = g.Key,
                        Revenue = g.Sum(b => b.TotalAmount),
                        Bookings = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };
        }

        public async Task<IEnumerable<MoviePopularityViewModel>> GetMoviePopularityReportAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == "Confirmed")
                .GroupBy(b => b.Screening.Movie)
                .Select(g => new MoviePopularityViewModel
                {
                    Movie = g.Key,
                    TotalBookings = g.Count(),
                    TotalTickets = g.Sum(b => b.NumberOfTickets),
                    TotalRevenue = g.Sum(b => b.TotalAmount),
                    AverageRating = g.Key.Rating
                })
                .OrderByDescending(m => m.TotalRevenue)
                .ToListAsync();
        }

        public async Task<IEnumerable<HallUtilizationViewModel>> GetHallUtilizationReportAsync()
        {
            var halls = await _context.CinemaHalls.Where(h => h.IsActive).ToListAsync();
            var result = new List<HallUtilizationViewModel>();

            foreach (var hall in halls)
            {
                var screenings = await _context.Screenings
                    .Where(s => s.HallId == hall.HallId && s.StartTime >= DateTime.Today.AddDays(-30))
                    .Include(s => s.Bookings)
                    .ToListAsync();

                var totalCapacity = screenings.Count * hall.Capacity;
                var bookedSeats = screenings.SelectMany(s => s.Bookings)
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.NumberOfTickets);

                result.Add(new HallUtilizationViewModel
                {
                    Hall = hall,
                    TotalScreenings = screenings.Count,
                    TotalCapacity = totalCapacity,
                    BookedSeats = bookedSeats,
                    UtilizationPercentage = totalCapacity > 0 ? (decimal)bookedSeats / totalCapacity * 100 : 0
                });
            }

            return result.OrderByDescending(h => h.UtilizationPercentage);
        }

        public async Task<SystemSettingsViewModel> GetSystemSettingsAsync()
        {
            // This would typically come from a Settings table or configuration
            return new SystemSettingsViewModel
            {
                CinemaName = "Multikino",
                DefaultTicketPrice = 25.00m,
                BookingTimeLimit = 30,
                CancellationTimeLimit = 2,
                MaxTicketsPerBooking = 10,
                EnableNotifications = true,
                MaintenanceMode = false
            };
        }

        public async Task<bool> UpdateSystemSettingsAsync(SystemSettingsViewModel model)
        {
            // Implementation would save to settings table or configuration
            // For now, just return true
            return true;
        }
    }
}