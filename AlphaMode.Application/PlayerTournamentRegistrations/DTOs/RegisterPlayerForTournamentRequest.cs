namespace AlphaMode.Application.PlayerTournamentRegistrations.DTOs;

public class RegisterPlayerForTournamentRequest
{
    public Guid TournamentId { get; set; }

    public Guid PlayerId { get; set; }
}