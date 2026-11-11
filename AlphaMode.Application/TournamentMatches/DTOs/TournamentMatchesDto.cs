using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentMatches.DTOs;

public class TournamentMatchesDto
{
    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public TournamentFormat Format { get; set; }

    public IReadOnlyList<TournamentMatchRoundDto> Rounds { get; set; } = [];
}

public class TournamentMatchRoundDto
{
    public int RoundNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<MatchDetailsDto> Matches { get; set; } = [];
}