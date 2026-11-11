using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Dashboards.DTOs;

public sealed class UserDashboardDto
{
    public string GamerTag { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public int MatchesPlayed { get; init; }
    public int Wins { get; init; }
    public int Draws { get; init; }
    public int Losses { get; init; }
    public decimal EloRating { get; init; } = 1000m;
    public int? Rank { get; init; }
    public IReadOnlyList<UserMatchDto> UpcomingMatches { get; init; } = [];
    public IReadOnlyList<UserTournamentSummaryDto> Tournaments { get; init; } = [];
    public IReadOnlyList<UserNotificationDto> Notifications { get; init; } = [];
}

public sealed class UserMatchDto
{
    public Guid Id { get; init; }
    public Guid? TournamentId { get; init; }
    public string CompetitionName { get; init; } = string.Empty;
    public string GameName { get; init; } = string.Empty;
    public string PlayerOneName { get; init; } = "TBD";
    public Guid? PlayerOneId { get; init; }
    public string PlayerTwoName { get; init; } = "TBD";
    public Guid? PlayerTwoId { get; init; }
    public Guid? WinnerId { get; init; }
    public int RoundNumber { get; init; }
    public MatchStatus Status { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int? PlayerOneScore { get; init; }
    public int? PlayerTwoScore { get; init; }
    public bool IsRanked { get; init; }
    public bool IsFriendly => !TournamentId.HasValue;
}

public sealed class UserTournamentSummaryDto
{
    public Guid TournamentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string GameName { get; init; } = string.Empty;
    public TournamentFormat Format { get; init; }
    public TournamentStatus Status { get; init; }
    public DateTime StartDate { get; init; }
    public decimal PrizePool { get; init; }
}

public sealed class UserNotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class UserFixturesDto
{
    public IReadOnlyList<UserTournamentSummaryDto> Tournaments { get; init; } = [];
    public IReadOnlyList<UserMatchDto> Matches { get; init; } = [];
}

public sealed class AdminDashboardDto
{
    public int ActivePlayers { get; init; }
    public int ActiveTournaments { get; init; }
    public int MatchesToday { get; init; }
    public int LiveMatches { get; init; }
    public int DraftTournaments { get; init; }
    public IReadOnlyList<UserMatchDto> LiveMatchItems { get; init; } = [];
}

