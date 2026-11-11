using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PlayerTournamentRegistrations.DTOs;

namespace AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;

public interface ITournamentRegistrationLock
{
    Task<Result<TournamentPlayerRegistrationDTO>> ExecuteAsync(
        Guid tournamentId,
        Func<CancellationToken,
            Task<Result<TournamentPlayerRegistrationDTO>>> operation,
        CancellationToken cancellationToken = default);
}
