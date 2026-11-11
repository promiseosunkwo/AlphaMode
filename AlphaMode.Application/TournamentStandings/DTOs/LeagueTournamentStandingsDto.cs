using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentStandings.DTOs;

public class LeagueTournamentStandingsDto
{
    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public TournamentFormat Format { get; set; }


    public IReadOnlyList<LeagueTournamentStandingDto> Standings { get; set; } = Array.Empty<LeagueTournamentStandingDto>();
}
