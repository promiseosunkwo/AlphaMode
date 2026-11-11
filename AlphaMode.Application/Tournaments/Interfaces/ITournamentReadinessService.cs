using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Tournaments.DTOs;

namespace AlphaMode.Application.Tournaments.Interfaces;

public interface ITournamentReadinessService
{
    Task<Result<TournamentReadinessDto>> CheckAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default);
}