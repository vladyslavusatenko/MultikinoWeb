using Microsoft.EntityFrameworkCore;
using MultikinoWeb.Data;
using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly MultikinoDbContext _context;

        public UserManagementService(MultikinoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersWithStatsAsync()
        {
            return await _context.Users
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            var bookings = user.Bookings.Where(b => b.Status == "Confirmed").ToList();

            return new UserStatisticsViewModel
            {
                User = user,
                TotalBookings = bookings.Count,
                TotalSpent = bookings.Sum(b => b.TotalAmount),
                TotalTickets = bookings.Sum(b => b.NumberOfTickets),
                LastBookingDate = bookings.Any() ? bookings.Max(b => b.BookingDate) : null,
                FavoriteGenre = bookings
                    .GroupBy(b => b.Screening.Movie.Genre)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "Brak danych",
                RecentBookings = bookings
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .ToList()
            };
        }

        public async Task<bool> BanUserAsync(int userId, string reason)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role == "Admin") return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AdminUsersOverviewViewModel> GetUsersOverviewAsync()
        {
            var allUsers = await _context.Users.ToListAsync();
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            return new AdminUsersOverviewViewModel
            {
                TotalUsers = allUsers.Count,
                ActiveUsers = allUsers.Count(u => u.IsActive),
                BannedUsers = allUsers.Count(u => !u.IsActive),
                AdminUsers = allUsers.Count(u => u.Role == "Admin"),
                NewUsersToday = allUsers.Count(u => u.CreatedAt.Date == today),
                NewUsersThisMonth = allUsers.Count(u => u.CreatedAt >= thisMonth),
                TopSpenders = await GetTopSpendersAsync()
            };
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Screening)  // Add this line
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role == "Admin")
                return false;

            var hasActiveBookings = user.Bookings.Any(b =>
                b.Status == "Confirmed" &&
                b.Screening != null && // Extra null check for safety
                b.Screening.StartTime > DateTime.Now);

            if (hasActiveBookings)
            {
                return false; // Cannot delete user with active bookings
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            return await _context.Users
                .Include(u => u.Bookings)
                .Where(u => u.FirstName.Contains(searchTerm) ||
                           u.LastName.Contains(searchTerm) ||
                           u.Email.Contains(searchTerm))
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        private async Task<List<TopSpenderViewModel>> GetTopSpendersAsync()
        {
            return await _context.Users
                .Where(u => u.Bookings.Any(b => b.Status == "Confirmed"))
                .Select(u => new TopSpenderViewModel
                {
                    User = u,
                    TotalSpent = u.Bookings
                        .Where(b => b.Status == "Confirmed")
                        .Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();
        }
    }
}