using MultikinoWeb.Models;

namespace MultikinoWeb.Services
{
    public interface IScreeningCleanupService
    {
        Task ProcessExpiredScreeningsAsync();
    }
}