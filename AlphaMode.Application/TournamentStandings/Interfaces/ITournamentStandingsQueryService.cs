using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentStandings.DTOs;

namespace AlphaMode.Application.TournamentStandings.Interfaces;

public interface ITournamentStandingsQueryService
{
    Task<Result> InitialiseAsync(Guid tournamentId, IReadOnlyCollection<Guid> playerIds, CancellationToken cancellationToken = default);
    Task<Result<LeagueTournamentStandingsDto>> GetAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}
