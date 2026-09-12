namespace AlphaMode.Services;

// Session-scoped prototype state; replace these methods with repository calls for permanent registrations.
public sealed class UserTournamentRegistrationState
{
    private readonly HashSet<int> registeredTournamentIds = [1];
    public bool IsRegistered(int tournamentId) => registeredTournamentIds.Contains(tournamentId);
    public void Register(int tournamentId) => registeredTournamentIds.Add(tournamentId);
}

