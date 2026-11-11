using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Entities;

namespace AlphaMode.Application.TournamentMatches.Interfaces;

public interface IMatchResultProcessor
{
    Task<Result> ProcessAsync(
    Match match,
    int playerOneScore,
    int playerTwoScore,
    bool wentToExtraTime = false,
    int? playerOnePenaltyScore = null,
    int? playerTwoPenaltyScore = null,
    CancellationToken cancellationToken = default);

    Task<Result> ReverseAsync(Match match, CancellationToken cancellationToken = default);
}