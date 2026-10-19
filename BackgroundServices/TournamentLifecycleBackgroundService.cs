using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Tournaments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.BackgroundServices;

public sealed class TournamentLifecycleBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<TournamentLifecycleBackgroundService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public TournamentLifecycleBackgroundService(IServiceScopeFactory scopeFactory, ILogger<TournamentLifecycleBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tournament lifecycle background service started.");

        // Run immediately on application startup.
        await RunSynchronisationAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSynchronisationAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }



    private async Task RunSynchronisationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var lifecycleService = scope.ServiceProvider.GetRequiredService<ITournamentLifecycleService>();

            var now = DateTime.UtcNow;

            var tournamentIds = await context.Tournaments.AsNoTracking().Where(t =>
                (
                    t.Status == TournamentStatus.Upcoming &&
                    (!t.RegistrationOpenDate.HasValue ||
                     t.RegistrationOpenDate.Value <= now) &&
                    (!t.RegistrationDeadline.HasValue ||
                     t.RegistrationDeadline.Value > now) &&
                    t.StartDate > now
                )
                ||
                (
                t.Status == TournamentStatus.RegistrationOpen &&
                (
                    (t.RegistrationDeadline.HasValue &&
                     t.RegistrationDeadline.Value <= now) ||
                    t.StartDate <= now
                )
                ))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

            foreach (var tournamentId in tournamentIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var result = await lifecycleService.SynchroniseStatusAsync(tournamentId, cancellationToken);

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Lifecycle synchronisation failed for tournament {TournamentId}: {Error}", tournamentId, result.Error);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to synchronise tournament {TournamentId}", tournamentId);
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Tournament lifecycle synchronisation failed.");
        }
    }
}
