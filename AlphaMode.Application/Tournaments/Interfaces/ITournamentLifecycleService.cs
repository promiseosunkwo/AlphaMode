using AlphaMode.Application.Common.Models;

namespace AlphaMode.Application.Tournaments.Interfaces;

public interface ITournamentLifecycleService
{
    Task<Result> PublishAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result> StartAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result> CompleteAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result<TournamentStatus>> SynchroniseStatusAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}