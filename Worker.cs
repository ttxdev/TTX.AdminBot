using TTX.AdminBot.Services;

namespace TTX.AdminBot;

public class Worker(ILogger<Worker> _logger, IServiceScopeFactory _scopes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        Bot bot = scope.ServiceProvider.GetRequiredService<Bot>();

        _logger.LogInformation("Starting...");
        await bot.Start();
    }
}
