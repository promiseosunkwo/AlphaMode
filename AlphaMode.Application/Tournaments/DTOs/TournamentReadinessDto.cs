namespace AlphaMode.Application.Tournaments.DTOs;

public sealed class TournamentReadinessDto
{
    public Guid TournamentId { get; set; }

    public bool IsReady { get; set; }

    public int RegisteredPlayers { get; set; }

    public int CheckedInPlayers { get; set; }

    public int FixturesCreated { get; set; }

    public List<string> Issues { get; set; } = new();
}