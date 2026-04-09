using IPBlocker.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IPBlocker.Infrastructure.BackgroundServices;

/// <summary>
/// Background hosted service that runs every 5 minutes to evict expired temporary blocks.
/// </summary>
public class TemporaryBlockCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TemporaryBlockCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public TemporaryBlockCleanupService(IServiceScopeFactory scopeFactory, ILogger<TemporaryBlockCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temporary block cleanup service started. Interval: {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITemporaryBlockRepository>();

                var removedCount = await repository.RemoveExpiredAsync();

                if (removedCount > 0)
                {
                    _logger.LogInformation("Cleanup service removed {Count} expired temporary block(s).", removedCount);
                }
                else
                {
                    _logger.LogDebug("Cleanup service ran — no expired blocks to remove.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown, expected
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in temporary block cleanup service.");
            }
        }

        _logger.LogInformation("Temporary block cleanup service stopped.");
    }
}
