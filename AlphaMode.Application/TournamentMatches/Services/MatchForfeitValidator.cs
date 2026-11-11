
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentMatches.Requests;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentMatches.Services;

public static class MatchForfeitValidator
{
    public static Result Validate(Match match, ForfeitMatchRequest request)
    {
        if (match.Status == MatchStatus.Completed)
        {
            return Result.Failure("A completed match cannot be forfeited.");
        }

        if (match.Score is not null)
        {
            return Result.Failure("A match with an existing score cannot be forfeited.");
        }

        if (match.Tournament.Status != TournamentStatus.Live)
        {
            return Result.Failure("Matches can only be forfeited while the tournament is live.");
        }

        if (match.PlayerOneId is null || match.PlayerTwoId is null)
        {
            return Result.Failure("Both players must be assigned before a forfeiture.");
        }

        if (request.ResultType != MatchResultType.Walkover && request.ResultType != MatchResultType.DoubleForfeit)
        {
            return Result.Failure("Select a valid forfeiture result.");
        }

        if (!Enum.IsDefined(typeof(MatchForfeitReason), request.Reason))
        {
            return Result.Failure("Select a valid forfeiture reason.");
        }

        if (request.ResultType == MatchResultType.Walkover)
        {
            if (!request.ForfeitingPlayerId.HasValue)
            {
                return Result.Failure("Select the player who forfeited.");
            }

            if (request.ForfeitingPlayerId != match.PlayerOneId &&
                request.ForfeitingPlayerId != match.PlayerTwoId)
            {
                return Result.Failure("The forfeiting player must belong to this match.");
            }
        }

        if (request.ResultType == MatchResultType.DoubleForfeit && request.ForfeitingPlayerId.HasValue)
        {
            return Result.Failure("Do not select an individual player for a double forfeiture.");
        }

        if (request.ResultType == MatchResultType.DoubleForfeit && match.Tournament.Format != TournamentFormat.League)
        {
            return Result.Failure("Double forfeitures are currently supported only in league tournaments.");
        }

        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            return Result.Failure("Provide an explanation for the forfeiture.");
        }

        if (request.Notes.Length > 1000)
        {
            return Result.Failure("Forfeiture notes cannot exceed 1000 characters.");
        }

        return Result.Success();
    }
}
