using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FriendlyMatches.DTOs;
using AlphaMode.Application.FriendlyMatches.Interfaces;
using AlphaMode.Application.FriendlyMatches.Requests;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.FriendlyMatches.Services;

public sealed class FriendlyMatchService : IFriendlyMatchService
{
    private readonly IApplicationDbContext _context;

    public FriendlyMatchService(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<FriendlyMatchDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var matches = await FriendlyQuery()
            .OrderByDescending(x => x.ScheduledAt ?? x.CreatedDate)
            .Select(Project())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FriendlyMatchDto>>.Success(matches);
    }

    public async Task<Result<IReadOnlyList<FriendlyMatchDto>>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<IReadOnlyList<FriendlyMatchDto>>.Failure("A signed-in player is required.");

        var matches = await FriendlyQuery()
            .Where(x => x.PlayerOne!.UserId == userId || x.PlayerTwo!.UserId == userId)
            .OrderByDescending(x => x.ScheduledAt ?? x.CreatedDate)
            .Select(Project())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FriendlyMatchDto>>.Success(matches);
    }

    public async Task<Result<Guid>> CreateAsync(CreateFriendlyMatchRequest request, CancellationToken cancellationToken = default)
    {
        var rules = FriendlyMatchRules.Validate(request.GameId, request.PlayerOneId, request.PlayerTwoId, request.Type);
        if (!rules.Succeeded) return Result<Guid>.Failure(rules.Error!);

        var gameExists = await _context.Games.AsNoTracking()
            .AnyAsync(x => x.Id == request.GameId && x.IsActive, cancellationToken);
        if (!gameExists) return Result<Guid>.Failure("The selected game is unavailable.");

        var activePlayerCount = await _context.Players.AsNoTracking()
            .CountAsync(x => (x.Id == request.PlayerOneId || x.Id == request.PlayerTwoId) && x.IsActive, cancellationToken);
        if (activePlayerCount != 2) return Result<Guid>.Failure("Both players must exist and be active.");

        var match = new Match
        {
            Id = Guid.NewGuid(),
            GameId = request.GameId,
            TournamentId = null,
            PlayerOneId = request.PlayerOneId,
            PlayerTwoId = request.PlayerTwoId,
            Type = request.Type,
            // The server owns this flag: browser input can never turn an MVP friendly into a ranked match.
            IsRanked = false,
            RoundNumber = 0,
            Status = request.ScheduledAt.HasValue ? MatchStatus.Scheduled : MatchStatus.Pending,
            ScheduledAt = request.ScheduledAt
        };

        _context.Matches.Add(match);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(match.Id, "Friendly match scheduled.");
    }

    public Task<Result> RecordResultAsync(RecordFriendlyResultRequest request, CancellationToken cancellationToken = default) =>
        SaveResultAsync(request, correction: false, cancellationToken);

    public Task<Result> CorrectResultAsync(RecordFriendlyResultRequest request, CancellationToken cancellationToken = default) =>
        SaveResultAsync(request, correction: true, cancellationToken);

    public async Task<Result> CancelAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == matchId && x.TournamentId == null, cancellationToken);
        if (match is null) return Result.Failure("Friendly match was not found.");
        if (match.Status == MatchStatus.Completed) return Result.Failure("A completed friendly cannot be cancelled.");

        match.Status = MatchStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Friendly match cancelled.");
    }

    private async Task<Result> SaveResultAsync(RecordFriendlyResultRequest request, bool correction, CancellationToken cancellationToken)
    {
        var match = await _context.Matches
            .Include(x => x.Score)
            .FirstOrDefaultAsync(x => x.Id == request.MatchId && x.TournamentId == null, cancellationToken);

        if (match is null) return Result.Failure("Friendly match was not found.");
        if (match.Status == MatchStatus.Cancelled) return Result.Failure("A cancelled friendly cannot receive a result.");
        if (!correction && match.Status == MatchStatus.Completed) return Result.Failure("This friendly already has a result.");
        if (correction && match.Status != MatchStatus.Completed) return Result.Failure("Only a completed friendly can be corrected.");

        if (match.Score is null)
        {
            match.Score = new MatchScore { Id = Guid.NewGuid(), MatchId = match.Id };
            _context.MatchScores.Add(match.Score);
        }

        match.Score.PlayerOneScore = request.PlayerOneScore;
        match.Score.PlayerTwoScore = request.PlayerTwoScore;
        match.WinnerId = request.PlayerOneScore == request.PlayerTwoScore
            ? null
            : request.PlayerOneScore > request.PlayerTwoScore ? match.PlayerOneId : match.PlayerTwoId;
        match.Status = MatchStatus.Completed;

        // Deliberately do not update Player ranking, Elo history, tournament standings, or competitive W/D/L.
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(correction ? "Friendly result corrected." : "Friendly result recorded.");
    }

    private IQueryable<Match> FriendlyQuery() => _context.Matches.AsNoTracking().Where(x => x.TournamentId == null);

    private static System.Linq.Expressions.Expression<Func<Match, FriendlyMatchDto>> Project() => x => new FriendlyMatchDto
    {
        Id = x.Id,
        GameId = x.GameId,
        GameName = x.Game.Name,
        PlayerOneId = x.PlayerOneId!.Value,
        PlayerOneName = x.PlayerOne!.GamerTag,
        PlayerTwoId = x.PlayerTwoId!.Value,
        PlayerTwoName = x.PlayerTwo!.GamerTag,
        Type = x.Type,
        Status = x.Status,
        ScheduledAt = x.ScheduledAt,
        PlayerOneScore = x.Score == null ? null : x.Score.PlayerOneScore,
        PlayerTwoScore = x.Score == null ? null : x.Score.PlayerTwoScore,
        WinnerId = x.WinnerId,
        IsRanked = x.IsRanked
    };
}

