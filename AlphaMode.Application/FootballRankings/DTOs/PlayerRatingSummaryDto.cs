namespace AlphaMode.Application.FootballRankings.DTOs;

public sealed class PlayerRatingSummaryDto
{
    public int Position { get; init; }
    public decimal EloRating { get; init; }
    public decimal? LastChange { get; init; }
    public int MatchesPlayed { get; init; }
    public int Wins { get; init; }
    public int Draws { get; init; }
    public int Losses { get; init; }
    public bool IsProvisional => MatchesPlayed < 5;
}

