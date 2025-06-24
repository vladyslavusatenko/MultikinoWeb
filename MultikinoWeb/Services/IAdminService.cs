using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public interface IAdminService
    {
        // Dashboard statistics
        Task<AdminDashboardViewModel> GetDashboardDataAsync();

        // Cinema Halls management
        Task<IEnumerable<CinemaHall>> GetAllHallsAsync();
        Task<CinemaHall?> GetHallByIdAsync(int id);
        Task<bool> CreateHallAsync(CreateHallViewModel model);
        Task<bool> UpdateHallAsync(CinemaHall hall);
        Task<bool> DeleteHallAsync(int id);

        // Screenings management
        Task<IEnumerable<Screening>> GetAllScreeningsAsync();
        Task<Screening?> GetScreeningByIdAsync(int id);
        Task<bool> CreateScreeningAsync(CreateScreeningViewModel model);
        Task<bool> UpdateScreeningAsync(Screening screening);
        Task<bool> DeleteScreeningAsync(int id);
        Task<bool> IsHallAvailableAsync(int hallId, DateTime startTime, DateTime endTime, int? excludeScreeningId = null);

        // Reports
        Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<MoviePopularityViewModel>> GetMoviePopularityReportAsync();
        Task<IEnumerable<HallUtilizationViewModel>> GetHallUtilizationReportAsync();

        // System settings
        Task<SystemSettingsViewModel> GetSystemSettingsAsync();
        Task<bool> UpdateSystemSettingsAsync(SystemSettingsViewModel model);
    }
}