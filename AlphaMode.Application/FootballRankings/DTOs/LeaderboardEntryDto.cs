namespace AlphaMode.Application.FootballRankings.DTOs;

public sealed class LeaderboardEntryDto
{
    public int Position { get; init; }
    public Guid PlayerId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string GamerTag { get; init; } = string.Empty;
    public decimal EloRating { get; init; }
    public int MatchesPlayed { get; init; }
    public int Wins { get; init; }
    public int Draws { get; init; }
    public int Losses { get; init; }
    public bool IsProvisional => MatchesPlayed < 5;
}

