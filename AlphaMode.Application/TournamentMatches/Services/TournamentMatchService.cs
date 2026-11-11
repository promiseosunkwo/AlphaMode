using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FootballRankings.Interfaces;
using AlphaMode.Application.TournamentMatches.DTOs;
using AlphaMode.Application.TournamentMatches.Interfaces;
using AlphaMode.Application.TournamentMatches.Requests;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlphaMode.Application.TournamentMatches.Services;

public class TournamentMatchService : ITournamentMatchService
{
    private readonly IApplicationDbContext _context;
    private readonly IMatchResultProcessor _resultProcessor;
    private readonly IApplicationTransaction _transaction;
    private readonly ITournamentStandingService _standingService;
    private readonly IFootballRatingService _ratingService;

    public TournamentMatchService(IApplicationDbContext context, IMatchResultProcessor resultProcessor,
        IApplicationTransaction transaction, ITournamentStandingService standingService,
        IFootballRatingService ratingService)
    {
        _context = context;
        _resultProcessor = resultProcessor;
        _transaction = transaction;
        _standingService = standingService;
        _ratingService = ratingService;
    }


    // =====================================================
    // TOURNAMENT MATCHES
    // =====================================================

    public async Task<Result<TournamentMatchesDto>> GetTournamentMatchesAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<TournamentMatchesDto>
                .Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments
                .AsNoTracking()
                .Where(x => x.Id == tournamentId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    GameName = x.Game.Name,
                    x.Format
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentMatchesDto>.Failure("Tournament was not found.");
        }

        // Sequential query because we're using one scoped DbContext.
        var matches = await _context.Matches
                .AsNoTracking()
                .Where(x => x.TournamentId == tournamentId)
                .OrderBy(x => x.RoundNumber)
                .ThenBy(x => x.MatchWeek)
                .ThenBy(x => x.CreatedDate)
                .Select(x =>
                    new MatchDetailsDto
                    {
                        Id = x.Id,

                        TournamentId = x.TournamentId!.Value,

                        TournamentName = x.Tournament.Name,

                        TournamentFormat = x.Tournament.Format,

                        PlayerOneId = x.PlayerOneId,

                        PlayerOneName = x.PlayerOne != null ? x.PlayerOne.GamerTag : "TBD",

                        PlayerTwoId = x.PlayerTwoId,

                        PlayerTwoName = x.PlayerTwo != null ? x.PlayerTwo.GamerTag : "TBD",

                        RoundNumber = x.RoundNumber,

                        MatchWeek = x.MatchWeek,

                        Status = x.Status,

                        ScheduledAt = x.ScheduledAt,

                        WinnerId = x.WinnerId,

                        PlayerOneScore = x.Score != null ? x.Score.PlayerOneScore : null,

                        PlayerTwoScore = x.Score != null ? x.Score.PlayerTwoScore : null,

                        WentToExtraTime = x.Score != null && x.Score.WentToExtraTime,

                        PlayerOnePenaltyScore = x.Score != null ? x.Score.PlayerOnePenaltyScore : null,

                        PlayerTwoPenaltyScore = x.Score != null ? x.Score.PlayerTwoPenaltyScore : null,

                        ResultType = x.ResultType,

                        ForfeitReason = x.ForfeitReason,

                        ForfeitingPlayerId = x.ForfeitingPlayerId,

                        ForfeitNotes = x.ForfeitNotes,

                        ForfeitedAt = x.ForfeitedAt,


                    }).ToListAsync(cancellationToken);

        var totalRounds = matches.Count == 0 ? 0 : matches.Max(x => x.RoundNumber);

        var rounds = matches
        .GroupBy(x => x.RoundNumber)
        .OrderBy(x => x.Key)
        .Select(group =>
            new TournamentMatchRoundDto
            {
                RoundNumber = group.Key,

                Name = GetRoundName(
                        tournament.Format,
                        group.Key,
                        totalRounds,
                        group.Count()),

                Matches = group.ToList()
            }).ToList();

        return Result<TournamentMatchesDto>.Success(
                new TournamentMatchesDto
                {
                    TournamentId = tournament.Id,

                    TournamentName = tournament.Name,

                    GameName = tournament.GameName,

                    Format = tournament.Format,

                    Rounds = rounds
                });
    }


    // =====================================================
    // GET ONE
    // =====================================================

    public async Task<Result<MatchDetailsDto>> GetByIdAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (matchId == Guid.Empty)
        {
            return Result<MatchDetailsDto>.Failure("Match is required.");
        }

        var match = await _context.Matches
                .AsNoTracking()
                .Where(x => x.Id == matchId)
                .Select(x =>
                    new MatchDetailsDto
                    {
                        Id = x.Id,

                        TournamentId = x.TournamentId!.Value,

                        TournamentName = x.Tournament.Name,

                        TournamentFormat = x.Tournament.Format,

                        PlayerOneId = x.PlayerOneId,

                        PlayerOneName = x.PlayerOne != null ? x.PlayerOne.GamerTag : "TBD",

                        PlayerTwoId = x.PlayerTwoId,

                        PlayerTwoName = x.PlayerTwo != null ? x.PlayerTwo.GamerTag : "TBD",

                        RoundNumber = x.RoundNumber,

                        MatchWeek = x.MatchWeek,

                        Status = x.Status,

                        ScheduledAt = x.ScheduledAt,

                        WinnerId = x.WinnerId,

                        PlayerOneScore = x.Score != null ? x.Score.PlayerOneScore : null,

                        PlayerTwoScore = x.Score != null ? x.Score.PlayerTwoScore : null,

                        WentToExtraTime = x.Score != null && x.Score.WentToExtraTime,

                        PlayerOnePenaltyScore = x.Score != null ? x.Score.PlayerOnePenaltyScore : null,

                        PlayerTwoPenaltyScore = x.Score != null ? x.Score.PlayerTwoPenaltyScore : null,

                        ResultType = x.ResultType,

                        ForfeitReason = x.ForfeitReason,

                        ForfeitingPlayerId = x.ForfeitingPlayerId,

                        ForfeitNotes = x.ForfeitNotes,

                        ForfeitedAt = x.ForfeitedAt,
                    })
                .FirstOrDefaultAsync(cancellationToken);

        if (match is null)
        {
            return Result<MatchDetailsDto>
                .Failure("Match was not found.");
        }

        return Result<MatchDetailsDto>
            .Success(match);
    }


    // =====================================================
    // SCHEDULE
    // =====================================================

    public async Task<Result> ScheduleAsync(ScheduleMatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MatchId == Guid.Empty)
        {
            return Result.Failure("Match is required.");
        }

        if (!request.ScheduledAt.HasValue)
        {
            return Result.Failure("Please select a match date and time.");
        }

        var match =
            await _context.Matches
                .FirstOrDefaultAsync(
                    x => x.Id == request.MatchId,
                    cancellationToken);

        if (match is null)
        {
            return Result.Failure("Match was not found.");
        }

        if (match.Status == MatchStatus.Completed)
        {
            return Result.Failure("A completed match cannot be rescheduled.");
        }

        if (match.Status == MatchStatus.Live)
        {
            return Result.Failure("A live match cannot be rescheduled.");
        }

        if (!match.PlayerOneId.HasValue || !match.PlayerTwoId.HasValue)
        {
            return Result.Failure("This match does not yet have two players.");
        }

        match.ScheduledAt = request.ScheduledAt.Value;

        match.Status = MatchStatus.Scheduled;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Match scheduled successfully.");
    }


    // =====================================================
    // START
    // =====================================================

    public async Task<Result> StartAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (matchId == Guid.Empty)
        {
            return Result.Failure("Match is required.");
        }

        var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == matchId, cancellationToken);

        if (match is null)
        {
            return Result.Failure("Match was not found.");
        }

        if (match.Status == MatchStatus.Completed)
        {
            return Result.Failure("This match has already been completed.");
        }

        if (match.Status == MatchStatus.Live)
        {
            return Result.Failure("This match is already live.");
        }

        if (match.Status != MatchStatus.Pending &&
            match.Status != MatchStatus.Scheduled)
        {
            return Result.Failure("This match cannot be started.");
        }

        if (!match.PlayerOneId.HasValue || !match.PlayerTwoId.HasValue)
        {
            return Result.Failure("Both players must be known before the match can start.");
        }

        match.Status = MatchStatus.Live;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Match started successfully.");
    }


    // =====================================================
    // RESULT
    // =====================================================



    public async Task<Result> SubmitResultAsync(SubmitMatchResultRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MatchId == Guid.Empty)
        {
            return Result.Failure("Match is required.");
        }

        if (request.PlayerOneScore is not int playerOneScore ||
            request.PlayerTwoScore is not int playerTwoScore)
        {
            return Result.Failure("Both player scores are required.");
        }

        return await _transaction.ExecuteAsync(async ct =>
        {
            var match = await _context.Matches
                .Include(x => x.Tournament)
                .Include(x => x.Score)
                .Include(x => x.PlayerOne)
                .Include(x => x.PlayerTwo)
                .FirstOrDefaultAsync(x => x.Id == request.MatchId, ct);

            if (match is null)
            {
                return Result.Failure("Match could not be found.");
            }


            //if (match.Tournament.Status == TournamentStatus.Completed)
            //{
            //    return Result.Failure(
            //        "Results cannot be submitted because this tournament has been completed.");
            //}

            //if (match.Tournament.Status != TournamentStatus.Live)
            //{
            //    return Result.Failure(
            //        "Match results can only be submitted while the tournament is live.");
            //}


            if (match.Status == MatchStatus.Completed)
            {
                return Result.Failure(
                    "This match has already been completed.");
            }

            if (match.PlayerOneId is null ||
                match.PlayerTwoId is null)
            {
                return Result.Failure(
                    "Both players must be assigned before submitting a result.");
            }

            // =============================================
            // VALIDATE MATCH RESULT
            // =============================================

            var validation = MatchScoreValidator.Validate(
                match.Tournament.Format,
                playerOneScore,
                playerTwoScore,
                request.WentToExtraTime,
                request.PlayerOnePenaltyScore,
                request.PlayerTwoPenaltyScore);

            if (!validation.Succeeded)
            {
                return validation;
            }

            // =============================================
            // PROCESS RESULT
            // =============================================

            var processResult = await _resultProcessor.ProcessAsync(
                match,
                playerOneScore,
                playerTwoScore,
                request.WentToExtraTime,
                request.PlayerOnePenaltyScore,
                request.PlayerTwoPenaltyScore,
                ct);

            if (!processResult.Succeeded)
            {
                return processResult;
            }

            // Elo participates in the same transaction as the score. Unranked matches return success
            // without side effects, while any genuine rating failure rolls the entire result back.
            var ratingResult = await _ratingService.ApplyMatchAsync(match, ct);
            if (!ratingResult.Succeeded)
            {
                return ratingResult;
            }

            // =============================================
            // SAVE RESULT BEFORE KNOCKOUT PROGRESSION
            // =============================================

            await _context.SaveChangesAsync(ct);

            // =============================================
            // PROGRESS SINGLE ELIMINATION
            // =============================================

            if (match.Tournament.Format ==
                TournamentFormat.SingleElimination)
            {
                var progressionResult =
                    await TryProgressSingleEliminationAsync(match, ct);

                if (!progressionResult.Succeeded)
                {
                    return progressionResult;
                }
            }

            return Result.Success("Match result submitted successfully.");

        }, cancellationToken);
    }




    // =====================================================
    // SINGLE ELIMINATION PROGRESSION
    // =====================================================

    private async Task<Result> TryProgressSingleEliminationAsync(Domain.Entities.Match completedMatch, CancellationToken cancellationToken)
    {
        var tournamentId = completedMatch.TournamentId;
        var completedRoundNumber = completedMatch.RoundNumber;

        var roundMatches = await _context.Matches
            .Where(x => x.TournamentId == tournamentId && x.RoundNumber == completedRoundNumber)
            .OrderBy(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

        if (roundMatches.Count == 0)
        {
            return Result.Success();
        }

        // Do not progress until every match
        // in the round has been completed.
        if (roundMatches.Any(x => x.Status != MatchStatus.Completed))
        {
            return Result.Success();
        }

        // Every elimination match must have a winner.
        if (roundMatches.Any(x => !x.WinnerId.HasValue))
        {
            return Result.Failure("The round is complete but one or more matches do not have a winner.");
        }

        // One match means this was the final.
        if (roundMatches.Count == 1)
        {
            return Result.Success("Tournament final completed.");
        }

        var nextRoundNumber = completedRoundNumber + 1;

        // Protect against duplicate next-round creation.
        var nextRoundAlreadyExists = await _context.Matches
                .AnyAsync(x => x.TournamentId == tournamentId && x.RoundNumber == nextRoundNumber, cancellationToken);

        if (nextRoundAlreadyExists)
        {
            return Result.Success();
        }

        var winners = roundMatches
            .Select(x => x.WinnerId!.Value)
            .ToList();

        if (winners.Count % 2 != 0)
        {
            return Result.Failure(
                "The completed round produced an invalid number of winners.");
        }

        var nextRoundMatches = new List<Match>();

        for (var i = 0; i < winners.Count; i += 2)
        {
            nextRoundMatches.Add(new Match
            {
                TournamentId = tournamentId,

                // Later knockout rounds inherit the direct game and ranked classification from the completed round.
                GameId = completedMatch.GameId,
                Type = AlphaMode.Domain.Enums.MatchType.Tournament,
                IsRanked = true,

                PlayerOneId = winners[i],
                PlayerTwoId = winners[i + 1],

                RoundNumber = nextRoundNumber,

                MatchWeek = null,

                Status = MatchStatus.Pending,

                ScheduledAt = null,

                WinnerId = null
            });
        }

        _context.Matches.AddRange(nextRoundMatches);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Round {nextRoundNumber} created successfully.");
    }

    // =====================================================
    // DISPLAY HELPERS
    // =====================================================

    private static string GetRoundName(
        TournamentFormat format,
        int roundNumber,
        int totalRounds,
        int matchesInRound)
    {
        if (format == TournamentFormat.League)
        {
            return $"Matchweek {roundNumber}";
        }

        return matchesInRound switch
        {
            1 => "Final",
            2 => "Semi-finals",
            4 => "Quarter-finals",
            8 => "Round of 16",
            16 => "Round of 32",
            _ => $"Round {roundNumber}"
        };
    }


    public async Task<Result> CorrectResultAsync(CorrectMatchResultRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MatchId == Guid.Empty)
        {
            return Result.Failure("Match is required.");
        }

        if (request.PlayerOneScore is not int playerOneScore || request.PlayerTwoScore is not int playerTwoScore)
        {
            return Result.Failure("Both player scores are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Failure("A reason for the correction is required.");
        }


        return await _transaction.ExecuteAsync(async ct =>
        {
            var match = await _context.Matches
                .Include(x => x.Tournament)
                .Include(x => x.Score)
                .Include(x => x.PlayerOne)
                .Include(x => x.PlayerTwo)
                .FirstOrDefaultAsync(x => x.Id == request.MatchId, ct);



            if (match is null)
            {
                return Result.Failure("Match could not be found.");
            }

            if (match.ResultType != MatchResultType.Played)
            {
                return Result.Failure("Forfeited matches cannot be corrected using normal score correction.");
            }


            if (match.Tournament.Status == TournamentStatus.Completed)
            {
                return Result.Failure("Results cannot be corrected because this tournament has been completed.");
            }

            if (match.Tournament.Status != TournamentStatus.Live)
            {
                return Result.Failure("Match results can only be corrected while the tournament is live.");
            }


            if (match.Status != MatchStatus.Completed)
            {
                return Result.Failure("Only completed matches can be corrected.");
            }


            if (match.Score is null)
            {
                return Result.Failure("The completed match does not have a score.");
            }


            if (match.PlayerOneId is null || match.PlayerTwoId is null)
            {
                return Result.Failure("Both players must be assigned.");
            }


            // Validate the corrected score, extra time and penalties
            var validation = MatchScoreValidator.Validate(
                match.Tournament.Format,
                playerOneScore,
                playerTwoScore,
                request.WentToExtraTime,
                request.PlayerOnePenaltyScore,
                request.PlayerTwoPenaltyScore);

            if (!validation.Succeeded)
            {
                return validation;
            }


            // =============================================
            // CHECK IF RESULT IS UNCHANGED
            // =============================================

            if (match.Score.PlayerOneScore == playerOneScore &&
                 match.Score.PlayerTwoScore == playerTwoScore &&
                 match.Score.WentToExtraTime == request.WentToExtraTime &&
                 match.Score.PlayerOnePenaltyScore == request.PlayerOnePenaltyScore &&
                 match.Score.PlayerTwoPenaltyScore == request.PlayerTwoPenaltyScore)
            {
                return Result.Failure(
                    "The corrected result is the same as the current result.");
            }


            // =============================================
            // CAPTURE ORIGINAL RESULT
            // =============================================

            var oldPlayerOneScore = match.Score.PlayerOneScore;
            var oldPlayerTwoScore = match.Score.PlayerTwoScore;
            var oldWentToExtraTime = match.Score.WentToExtraTime;
            var oldPlayerOnePenaltyScore = match.Score.PlayerOnePenaltyScore;
            var oldPlayerTwoPenaltyScore = match.Score.PlayerTwoPenaltyScore;
            var oldWinnerId = match.WinnerId;


            var newWinnerId = DetermineWinner(
                match.PlayerOneId.Value,
                match.PlayerTwoId.Value,
                playerOneScore,
                playerTwoScore,
                request.PlayerOnePenaltyScore,
                request.PlayerTwoPenaltyScore);


            // =============================================
            // VALIDATE BRACKET BEFORE CHANGING ANYTHING
            // =============================================

            if (match.Tournament.Format == TournamentFormat.SingleElimination && oldWinnerId != newWinnerId)
            {
                var validationResult = await ValidateKnockoutCorrectionAsync(
                        match,
                        oldWinnerId,
                        ct);

                if (!validationResult.Succeeded)
                {
                    return validationResult;
                }
            }


            // =============================================
            // REVERSE ORIGINAL RESULT
            // =============================================

            var reverseResult = await _resultProcessor.ReverseAsync(match, ct);

            if (!reverseResult.Succeeded)
            {
                return reverseResult;
            }


            // =============================================
            // APPLY CORRECTED RESULT
            // =============================================

            var processResult = await _resultProcessor.ProcessAsync(
                    match,
                    playerOneScore,
                    playerTwoScore,
                    request.WentToExtraTime,
                    request.PlayerOnePenaltyScore,
                    request.PlayerTwoPenaltyScore,
                    ct);

            if (!processResult.Succeeded)
            {
                return processResult;
            }


            // =============================================
            // REPAIR KNOCKOUT PROGRESSION
            // =============================================

            if (match.Tournament.Format == TournamentFormat.SingleElimination && oldWinnerId != match.WinnerId)
            {
                var progressionResult = await CorrectSingleEliminationProgressionAsync(
                        match,
                        oldWinnerId,
                        ct);

                if (!progressionResult.Succeeded)
                {
                    return progressionResult;
                }
            }


            // =============================================
            // AUDIT
            // =============================================

            AddResultCorrectionAudit(
                match,
                oldPlayerOneScore,
                oldPlayerTwoScore,
                oldWentToExtraTime,
                oldPlayerOnePenaltyScore,
                oldPlayerTwoPenaltyScore,
                playerOneScore,
                playerTwoScore,
                oldWinnerId,
                request.Reason.Trim());

            // A correction changes the starting rating of every later match in this game.
            // Replay the game ledger in its original sequence instead of subtracting one old delta.
            var rebuildResult = await _ratingService.RebuildGameAsync(match.GameId, ct);
            if (!rebuildResult.Succeeded)
            {
                return rebuildResult;
            }


            // =============================================
            // ONE DATABASE SAVE
            // =============================================

            await _context.SaveChangesAsync(ct);


            return Result.Success("Match result corrected successfully.");
        }, cancellationToken);
    }




    //HELPERS
    private static Guid? DetermineWinner(
    Guid playerOneId,
    Guid playerTwoId,
    int playerOneScore,
    int playerTwoScore,
    int? playerOnePenaltyScore,
    int? playerTwoPenaltyScore)
    {
        if (playerOneScore > playerTwoScore)
            return playerOneId;

        if (playerTwoScore > playerOneScore)
            return playerTwoId;

        if (playerOnePenaltyScore.HasValue &&
            playerTwoPenaltyScore.HasValue)
        {
            return playerOnePenaltyScore.Value > playerTwoPenaltyScore.Value
                ? playerOneId
                : playerTwoId;
        }

        return null;
    }

    private async Task<Result> ValidateKnockoutCorrectionAsync(
    Match match,
    Guid? oldWinnerId,
    CancellationToken cancellationToken)
    {
        if (!oldWinnerId.HasValue)
        {
            return Result.Success();
        }

        var downstreamMatch = await _context.Matches.FirstOrDefaultAsync(
        x =>
            x.TournamentId == match.TournamentId &&
            x.RoundNumber == match.RoundNumber + 1 &&
            (
                x.PlayerOneId == oldWinnerId.Value ||
                x.PlayerTwoId == oldWinnerId.Value
            ),
        cancellationToken);

        if (downstreamMatch is null)
        {
            return Result.Success();
        }

        if (downstreamMatch.Status == MatchStatus.Live ||
            downstreamMatch.Status == MatchStatus.Completed)
        {
            return Result.Failure(
                "The result cannot be corrected because the " +
                "affected next-round match has already started or completed.");
        }

        return Result.Success();
    }


    //Match Fortfiture
    public async Task<Result> ForfeitMatchAsync(ForfeitMatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MatchId == Guid.Empty)
        {
            return Result.Failure("Match is required.");
        }

        return await _transaction.ExecuteAsync(async ct =>
        {
            var match = await _context.Matches
                .Include(x => x.Tournament)
                .Include(x => x.Score)
                .Include(x => x.PlayerOne)
                .Include(x => x.PlayerTwo)
                .FirstOrDefaultAsync(x => x.Id == request.MatchId, ct);

            if (match is null)
            {
                return Result.Failure("Match could not be found.");
            }

            // Validate before modifying any data.
            var validation = MatchForfeitValidator.Validate(match, request);

            if (!validation.Succeeded)
            {
                return validation;
            }

            // A forfeiture is not a played match.
            // Do not create a MatchScore or call MatchResultProcessor.

            Guid? winnerId = null;

            if (request.ResultType == MatchResultType.Walkover)
            {
                winnerId = request.ForfeitingPlayerId == match.PlayerOneId
                    ? match.PlayerTwoId
                    : match.PlayerOneId;
            }

            match.ResultType = request.ResultType;
            match.ForfeitReason = request.Reason;
            match.ForfeitingPlayerId = request.ForfeitingPlayerId;
            match.ForfeitNotes = request.Notes!.Trim();
            match.ForfeitedAt = DateTime.UtcNow;

            match.WinnerId = winnerId;
            match.Status = MatchStatus.Completed;

            if (match.Tournament.Format == TournamentFormat.League)
            {
                var standingResult = await _standingService.ApplyForfeitAsync(match, ct);

                if (!standingResult.Succeeded)
                {
                    return standingResult;
                }
            }

            //Audit log the reason for match forfeiture
            AddMatchForfeitureAudit(match);

            // Save the completed match before checking whether
            // the knockout round is ready to progress.
            await _context.SaveChangesAsync(ct);

            if (match.Tournament.Format == TournamentFormat.SingleElimination)
            {
                var progressionResult = await TryProgressSingleEliminationAsync(match, ct);

                if (!progressionResult.Succeeded)
                {
                    return progressionResult;
                }
            }

            return Result.Success(request.ResultType == MatchResultType.Walkover ? "Match completed by walkover."
                                                                      : "Match completed as a double forfeiture.");

        }, cancellationToken);
    }



    private async Task<Result> CorrectSingleEliminationProgressionAsync(Match correctedMatch, Guid? oldWinnerId, CancellationToken cancellationToken)
    {
        if (!oldWinnerId.HasValue ||
            !correctedMatch.WinnerId.HasValue)
        {
            return Result.Failure(
                "The corrected knockout match must have a winner.");
        }

        var downstreamMatch =
            await _context.Matches
                .FirstOrDefaultAsync(
                x => x.TournamentId == correctedMatch.TournamentId && x.RoundNumber == correctedMatch.RoundNumber + 1 &&
                    (
                        x.PlayerOneId == oldWinnerId.Value || x.PlayerTwoId == oldWinnerId.Value
                    ), cancellationToken);

        if (downstreamMatch is null)
        {
            return Result.Success();
        }

        if (downstreamMatch.Status == MatchStatus.Live || downstreamMatch.Status == MatchStatus.Completed)
        {
            return Result.Failure("The affected next-round match has already started.");
        }

        if (downstreamMatch.PlayerOneId == oldWinnerId.Value)
        {
            downstreamMatch.PlayerOneId = correctedMatch.WinnerId.Value;
        }
        else if (downstreamMatch.PlayerTwoId == oldWinnerId.Value)
        {
            downstreamMatch.PlayerTwoId = correctedMatch.WinnerId.Value;
        }
        else
        {
            return Result.Failure("The progressing player could not be found " + "in the next-round match.");
        }

        return Result.Success();
    }


    private void AddResultCorrectionAudit(
        Match match,
        int oldPlayerOneScore,
        int oldPlayerTwoScore,
        bool oldWentToExtraTime,
        int? oldPlayerOnePenaltyScore,
        int? oldPlayerTwoPenaltyScore,
        int newPlayerOneScore,
        int newPlayerTwoScore,
        Guid? oldWinnerId,
        string reason)
    {
        var changes = new
        {
            TournamentId = match.TournamentId,

            PlayerOne = new
            {
                Id = match.PlayerOneId,
                GamerTag = match.PlayerOne?.GamerTag
            },

            PlayerTwo = new
            {
                Id = match.PlayerTwoId,
                GamerTag = match.PlayerTwo?.GamerTag
            },

            OldResult = new
            {
                PlayerOneScore = oldPlayerOneScore,
                PlayerTwoScore = oldPlayerTwoScore,
                WentToExtraTime = oldWentToExtraTime,
                PlayerOnePenaltyScore = oldPlayerOnePenaltyScore,
                PlayerTwoPenaltyScore = oldPlayerTwoPenaltyScore,
                WinnerId = oldWinnerId
            },

            NewResult = new
            {
                PlayerOneScore = newPlayerOneScore,
                PlayerTwoScore = newPlayerTwoScore,
                WentToExtraTime = match.Score?.WentToExtraTime,
                PlayerOnePenaltyScore = match.Score?.PlayerOnePenaltyScore,
                PlayerTwoPenaltyScore = match.Score?.PlayerTwoPenaltyScore,
                WinnerId = match.WinnerId
            },

            Reason = reason
        };

        var auditLog = new AuditLog
        {
            Action = "MatchResultCorrected",
            EntityName = nameof(Match),
            EntityId = match.Id.ToString(),
            Changes = JsonSerializer.Serialize(changes)
        };

        _context.AuditLogs.Add(auditLog);
    }




    private void AddMatchForfeitureAudit(Match match)
    {
        var changes = new
        {
            TournamentId = match.TournamentId,
            MatchId = match.Id,

            PlayerOne = new
            {
                Id = match.PlayerOneId,
                GamerTag = match.PlayerOne?.GamerTag
            },

            PlayerTwo = new
            {
                Id = match.PlayerTwoId,
                GamerTag = match.PlayerTwo?.GamerTag
            },

            ResultType = match.ResultType.ToString(),
            Reason = match.ForfeitReason?.ToString(),
            ForfeitingPlayerId = match.ForfeitingPlayerId,
            WinnerId = match.WinnerId,
            Notes = match.ForfeitNotes,
            ForfeitedAt = match.ForfeitedAt
        };

        _context.AuditLogs.Add(new AuditLog
        {
            Action = "MatchForfeited",
            EntityName = nameof(Match),
            EntityId = match.Id.ToString(),
            Changes = JsonSerializer.Serialize(changes)
        });
    }

}

