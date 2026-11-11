using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PlayerTournamentRegistrations.DTOs;

namespace AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;

public interface ITournamentPlayerRegistrationService
{
    Task<Result<TournamentPlayerRegistrationDTO>> RegisterAsync(
        RegisterPlayerForTournamentRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TournamentPlayerRegistrationDTO>> GetByIdAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TournamentPlayerRegistrationDTO>>>
        GetTournamentRegistrationsAsync(
            Guid tournamentId,
            CancellationToken cancellationToken = default);

    Task<Result> CheckInAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    Task<Result> WithdrawAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default);

    Task<Result> DisqualifyAsync(
        Guid registrationId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result<TournamentRegistrationAvailabilityDto>> GetAvailabilityAsync(
    Guid tournamentId,
    Guid playerId,
    CancellationToken cancellationToken = default);

    Task<Result<TournamentRegistrationAvailabilityDto>> GetAvailabilityByRegistrationAsync(
    Guid registrationId,
    string userId,
    CancellationToken cancellationToken = default);

    Task<Result> PlayerCheckInAsync(
    Guid registrationId,
    string userId,
    CancellationToken cancellationToken = default);

    Task<Result> PlayerWithdrawAsync(
    Guid registrationId,
    string userId,
    CancellationToken cancellationToken = default);
}