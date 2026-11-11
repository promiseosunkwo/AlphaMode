using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentRegistration.DTOs;
using AlphaMode.Application.TournamentRegistration.Requests;
using AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;
using AlphaMode.Application.TournamentRegistrations.DTOs;
using AlphaMode.Application.TournamentRegistrations.Requests;
using AlphaMode.Components.Pages.UserPages;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrations.Interfaces;

public interface ITournamentRegistrationService
{
    Task<Result<PagedResult<TournamentRegistrationDto>>> GetPagedAsync(
        int page = 1,
        int pageSize = 25,
        string? search = null,
        Guid? tournamentId = null,
        TournamentRegistrationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<Result<TournamentRegistrationDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TournamentRegistrationTournamentDto>>>GetTournamentOptionsAsync(CancellationToken cancellationToken = default);

    Task<Result> MarkRegisteredAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> CheckInAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> WithdrawAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> DisqualifyAsync(DisqualifyRegistrationRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<UserTournamentDto>>> GetAvailableForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<Result<TournamentRegistrationResultDto>> RegisterForTournamentAsync(RegisterForTournamentRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MyTournamentDto>>> GetMyTournamentsAsync(string userId, CancellationToken cancellationToken = default);

    Task<Result<MyTournamentDto>> GetMyTournamentAsync(Guid tournamentId, string userId, CancellationToken cancellationToken = default);

    Task<Result> PlayerCheckInAsync(Guid registrationId, string userId, CancellationToken cancellationToken = default);

    Task<Result<MyTournamentDetailsDto>>GetPlayerTournamentDetailsAsync(Guid tournamentId, string userId, CancellationToken cancellationToken = default);

    Task<Result<AdminTournamentRegisteredParticipantsDto>>GetAdminTournamentParticipantsAsync(Guid tournamentId, CancellationToken cancellationToken = default);
}
