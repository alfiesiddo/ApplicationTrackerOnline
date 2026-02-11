using ApplicationTrackerOnline.Data;

namespace ApplicationTrackerOnline.Services
{
    public class AutomatedEmailService : BackgroundService
    {
        private readonly ILogger<AutomatedEmailService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AutomatedEmailService(ILogger<AutomatedEmailService> logger, IServiceScopeFactory scopeFactory) 
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    }
                    _logger.LogInformation("Emails sent correctly.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured sending all users their emails.");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
