namespace AlphaMode.Application.TournamentStandings.DTOs;

public class LeagueTournamentStandingDto
{
    public Guid PlayerId { get; set; }

    public string GamerTag { get; set; } = string.Empty;

    public int Position { get; set; }

    public int Played { get; set; }

    public int Wins { get; set; }

    public int Draws { get; set; }

    public int Losses { get; set; }

    public int ScoreFor { get; set; }

    public int ScoreAgainst { get; set; }

    public int ScoreDifference { get; set; }

    public int Points { get; set; }
}
