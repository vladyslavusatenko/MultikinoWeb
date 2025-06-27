namespace MultikinoWeb.Services
{
    public class ScreeningCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScreeningCleanupBackgroundService> _logger;

        public ScreeningCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ScreeningCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cleanupService = scope.ServiceProvider.GetRequiredService<IScreeningCleanupService>();

                    await cleanupService.ProcessExpiredScreeningsAsync();

                    // Sprawdzaj co godzinę
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd w ScreeningCleanupBackgroundService");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}