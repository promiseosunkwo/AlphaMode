using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentMatches.DTOs;
using AlphaMode.Application.TournamentMatches.Requests;
using AlphaMode.Domain.Entities;

namespace AlphaMode.Application.TournamentMatches.Interfaces;

public interface ITournamentMatchService
{
    Task<Result<TournamentMatchesDto>> GetTournamentMatchesAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result<MatchDetailsDto>> GetByIdAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> ScheduleAsync(ScheduleMatchRequest request, CancellationToken cancellationToken = default);

    Task<Result> StartAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<Result> SubmitResultAsync(SubmitMatchResultRequest request, CancellationToken cancellationToken = default);

    Task<Result> CorrectResultAsync(CorrectMatchResultRequest request, CancellationToken cancellationToken = default);

    Task<Result> ForfeitMatchAsync(ForfeitMatchRequest request, CancellationToken cancellationToken = default);
}