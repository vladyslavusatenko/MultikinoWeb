using MultikinoWeb.Models;
using MultikinoWeb.ViewModels;

namespace MultikinoWeb.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();

        Task<IEnumerable<CinemaHall>> GetAllHallsAsync();
        Task<CinemaHall?> GetHallByIdAsync(int id);
        Task<bool> CreateHallAsync(CreateHallViewModel model);
        Task<bool> UpdateHallAsync(CinemaHall hall);
        Task<bool> DeleteHallAsync(int id);

        Task<IEnumerable<Screening>> GetAllScreeningsAsync();
        Task<Screening?> GetScreeningByIdAsync(int id);
        Task<bool> CreateScreeningAsync(CreateScreeningViewModel model);
        Task<bool> UpdateScreeningAsync(Screening screening);
        Task<bool> DeleteScreeningAsync(int id);
        Task<bool> IsHallAvailableAsync(int hallId, DateTime startTime, DateTime endTime, int? excludeScreeningId = null);

        Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<MoviePopularityViewModel>> GetMoviePopularityReportAsync();
        Task<IEnumerable<HallUtilizationViewModel>> GetHallUtilizationReportAsync();

        Task<SystemSettingsViewModel> GetSystemSettingsAsync();
        Task<bool> UpdateSystemSettingsAsync(SystemSettingsViewModel model);
    }
}