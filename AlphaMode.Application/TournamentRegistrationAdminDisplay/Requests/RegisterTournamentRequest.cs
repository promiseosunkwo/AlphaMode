namespace AlphaMode.Application.TournamentRegistration.Requests;

public class RegisterForTournamentRequest
{
    public Guid TournamentId { get; set; }

    public string UserId { get; set; } = string.Empty;
}
