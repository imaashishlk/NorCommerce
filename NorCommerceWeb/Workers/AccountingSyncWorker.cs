using NorCommerce.Business.Services.IServices;

namespace NorCommerceWeb.Workers
{
    public class AccountingSyncWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AccountingSyncWorker> _logger;

        public AccountingSyncWorker(IServiceScopeFactory scopeFactory, ILogger<AccountingSyncWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            do
            {
                await SyncPendingOrdersAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task SyncPendingOrdersAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Get the list
                List<int> ids;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var accounting = scope.ServiceProvider.GetRequiredService<IAccountingService>();
                    ids = await accounting.GetPendingOrderIdsAsync();
                }

                // Sync each order in its own scope
                foreach (var id in ids)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var accounting = scope.ServiceProvider.GetRequiredService<IAccountingService>();

                        await accounting.SyncOrderAsync(id);
                        _logger.LogInformation("Accounting sync done for order {OrderId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Accounting sync failed for order {OrderId}. Will retry.", id);
                    }
                }
            }
            catch (Exception ex)
            {
                // Never let the worker die (e.g. DB down)
                _logger.LogError(ex, "Accounting sync round failed. Will retry.");
            }
        }
    }
}
