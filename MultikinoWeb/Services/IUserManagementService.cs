using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public interface IUserManagementService
    {
        Task<IEnumerable<User>> GetAllUsersWithStatsAsync();
        Task<UserStatisticsViewModel> GetUserStatisticsAsync(int userId);
        Task<bool> BanUserAsync(int userId, string reason);
        Task<bool> UnbanUserAsync(int userId);
        Task<bool> ChangeUserRoleAsync(int userId, string newRole);
        Task<AdminUsersOverviewViewModel> GetUsersOverviewAsync();
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
    }
}