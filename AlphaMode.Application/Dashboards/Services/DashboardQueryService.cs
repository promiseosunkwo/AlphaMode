using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Dashboards.DTOs;
using AlphaMode.Application.Dashboards.Interfaces;
using AlphaMode.Application.FootballRankings.Services;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AlphaMode.Application.Dashboards.Services;

public sealed class DashboardQueryService : IDashboardQueryService
{
    private readonly IApplicationDbContext _context;
    public DashboardQueryService(IApplicationDbContext context) => _context = context;

    public async Task<Result<UserDashboardDto>> GetUserDashboardAsync(string userId, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players.AsNoTracking().Where(x => x.UserId == userId)
            .Select(x => new { x.Id, x.GamerTag, x.User.FirstName, x.User.LastName, x.User.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken);
        if (player is null) return Result<UserDashboardDto>.Failure("A player profile is required to view the dashboard.");

        var matches = await LoadMatchesAsync(player.Id, cancellationToken);
        var primaryRanking = await _context.Rankings.AsNoTracking().Where(x => x.PlayerId == player.Id)
            .OrderByDescending(x => x.MatchesPlayed).ThenByDescending(x => x.EloRating).FirstOrDefaultAsync(cancellationToken);
        int? rank = primaryRanking is null ? null : await _context.Rankings.AsNoTracking()
            .CountAsync(x => x.GameId == primaryRanking.GameId && x.EloRating > primaryRanking.EloRating, cancellationToken) + 1;

        var tournaments = await LoadTournamentsAsync(player.Id, cancellationToken);
        var notifications = await _context.Notifications.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedDate).Take(5)
            .Select(x => new UserNotificationDto { Id = x.Id, Title = x.Title, Message = x.Message, IsRead = x.IsRead, CreatedAt = x.CreatedDate })
            .ToListAsync(cancellationToken);

        var completed = matches.Where(x => x.Status == MatchStatus.Completed).ToList();
        // WinnerId also covers penalties and walkovers, unlike comparing regulation scores.
        var wins = completed.Count(x => x.WinnerId == player.Id);
        var draws = completed.Count(x => !x.WinnerId.HasValue && x.PlayerOneScore == x.PlayerTwoScore);
        return Result<UserDashboardDto>.Success(new UserDashboardDto
        {
            GamerTag = player.GamerTag, DisplayName = $"{player.FirstName} {player.LastName}".Trim(), AvatarUrl = player.AvatarUrl,
            MatchesPlayed = completed.Count, Wins = wins, Draws = draws, Losses = Math.Max(0, completed.Count - wins - draws),
            EloRating = primaryRanking?.EloRating ?? EloCalculator.InitialRating, Rank = rank,
            UpcomingMatches = matches.Where(x => x.Status is MatchStatus.Pending or MatchStatus.Scheduled or MatchStatus.Live).OrderBy(x => x.ScheduledAt).Take(3).ToList(),
            Tournaments = tournaments.Take(3).ToList(), Notifications = notifications
        });
    }

    public async Task<Result<IReadOnlyList<UserMatchDto>>> GetUserMatchesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var playerId = await _context.Players.AsNoTracking().Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(cancellationToken);
        if (!playerId.HasValue) return Result<IReadOnlyList<UserMatchDto>>.Failure("A player profile could not be found.");
        return Result<IReadOnlyList<UserMatchDto>>.Success(await LoadMatchesAsync(playerId.Value, cancellationToken));
    }

    public async Task<Result<UserFixturesDto>> GetUserFixturesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var playerId = await _context.Players.AsNoTracking().Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(cancellationToken);
        if (!playerId.HasValue) return Result<UserFixturesDto>.Failure("A player profile could not be found.");
        return Result<UserFixturesDto>.Success(new UserFixturesDto { Tournaments = await LoadTournamentsAsync(playerId.Value, cancellationToken), Matches = (await LoadMatchesAsync(playerId.Value, cancellationToken)).Where(x => x.TournamentId.HasValue).ToList() });
    }

    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var live = await _context.Matches.AsNoTracking().Where(x => x.Status == MatchStatus.Live)
            .OrderBy(x => x.ScheduledAt).Take(5).Select(MatchProjection).ToListAsync(cancellationToken);
        var dto = new AdminDashboardDto
        {
            ActivePlayers = await _context.Players.CountAsync(x => x.IsActive, cancellationToken),
            ActiveTournaments = await _context.Tournaments.CountAsync(x => x.Status == TournamentStatus.Live, cancellationToken),
            DraftTournaments = await _context.Tournaments.CountAsync(x => x.Status == TournamentStatus.Draft, cancellationToken),
            MatchesToday = await _context.Matches.CountAsync(x => x.ScheduledAt >= today && x.ScheduledAt < tomorrow, cancellationToken),
            LiveMatches = live.Count, LiveMatchItems = live
        };
        return Result<AdminDashboardDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<UserMatchDto>>> GetPublicTournamentMatchesAsync(CancellationToken cancellationToken = default)
    {
        // Standalone friendlies are intentionally excluded from the public match centre.
        var matches = await _context.Matches.AsNoTracking().Where(x => x.TournamentId.HasValue && x.Tournament!.Status != TournamentStatus.Draft && x.Tournament.Status != TournamentStatus.Cancelled)
            .OrderByDescending(x => x.Status == MatchStatus.Live).ThenBy(x => x.ScheduledAt).Take(200)
            .Select(MatchProjection).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<UserMatchDto>>.Success(matches);
    }

    public async Task<Result<PublicHomeStatsDto>> GetPublicHomeStatsAsync(CancellationToken cancellationToken = default)
    {
        // Each figure is computed from persisted records; draft/cancelled competitions are not advertised publicly.
        var stats = new PublicHomeStatsDto
        {
            ActivePlayers = await _context.Players.AsNoTracking().CountAsync(x => x.IsActive, cancellationToken),
            PublishedTournaments = await _context.Tournaments.AsNoTracking().CountAsync(
                x => x.Status != TournamentStatus.Draft && x.Status != TournamentStatus.Cancelled, cancellationToken),
            CompletedMatches = await _context.Matches.AsNoTracking().CountAsync(
                x => x.TournamentId.HasValue && x.Status == MatchStatus.Completed, cancellationToken),
            PrizesPaid = await _context.Payments.AsNoTracking()
                .Where(x => x.Type == PaymentType.PrizePayout && x.Status == PaymentStatus.Completed)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m
        };

        return Result<PublicHomeStatsDto>.Success(stats);
    }

    private async Task<IReadOnlyList<UserMatchDto>> LoadMatchesAsync(Guid playerId, CancellationToken cancellationToken) =>
        await _context.Matches.AsNoTracking().Where(x => x.PlayerOneId == playerId || x.PlayerTwoId == playerId)
            .OrderByDescending(x => x.Status == MatchStatus.Live).ThenByDescending(x => x.ScheduledAt)
            .Select(MatchProjection).ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<UserTournamentSummaryDto>> LoadTournamentsAsync(Guid playerId, CancellationToken cancellationToken) =>
        await _context.TournamentRegistrations.AsNoTracking()
            .Where(x => x.PlayerId == playerId && x.Status != TournamentRegistrationStatus.Withdrawn && x.Status != TournamentRegistrationStatus.Cancelled && x.Status != TournamentRegistrationStatus.Disqualified)
            .OrderBy(x => x.Tournament.StartDate)
            .Select(x => new UserTournamentSummaryDto { TournamentId = x.TournamentId, Name = x.Tournament.Name, GameName = x.Tournament.Game.Name, Format = x.Tournament.Format, Status = x.Tournament.Status, StartDate = x.Tournament.StartDate, PrizePool = x.Tournament.EntryFee * x.Tournament.Registrations.Count })
            .ToListAsync(cancellationToken);

    // Keeping this as an expression allows EF Core to translate the shared match projection to SQL.
    private static readonly Expression<Func<AlphaMode.Domain.Entities.Match, UserMatchDto>> MatchProjection = x => new()
    {
        Id = x.Id, TournamentId = x.TournamentId, CompetitionName = x.Tournament != null ? x.Tournament.Name : x.Type.ToString(),
        GameName = x.Game.Name, PlayerOneId = x.PlayerOneId, PlayerOneName = x.PlayerOne != null ? x.PlayerOne.GamerTag : "TBD", PlayerTwoId = x.PlayerTwoId, PlayerTwoName = x.PlayerTwo != null ? x.PlayerTwo.GamerTag : "TBD", WinnerId = x.WinnerId,
        RoundNumber = x.RoundNumber, Status = x.Status, ScheduledAt = x.ScheduledAt, PlayerOneScore = x.Score != null ? x.Score.PlayerOneScore : null,
        PlayerTwoScore = x.Score != null ? x.Score.PlayerTwoScore : null, IsRanked = x.IsRanked
    };
}
