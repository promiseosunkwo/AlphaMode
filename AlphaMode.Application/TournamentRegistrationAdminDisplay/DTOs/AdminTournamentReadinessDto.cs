
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;

public class AdminTournamentReadinessDto
{
    public Guid TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public string GameName { get; set; } = string.Empty;

    public TournamentFormat Format { get; set; }

    public TournamentStatus TournamentStatus { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxPlayers { get; set; }

    public int TotalRegistrations { get; set; }

    public int PendingPayment { get; set; }

    public int Registered { get; set; }

    public int CheckedIn { get; set; }

    public int Withdrawn { get; set; }

    public int Disqualified { get; set; }

    public int Cancelled { get; set; }

    public int ConfirmedPlayers => Registered + CheckedIn;

    public int AvailablePlaces => Math.Max(0, MaxPlayers - ActiveReservations);

    public int ActiveReservations => PendingPayment + Registered + CheckedIn;

    public int EligiblePlayers => CheckedIn;

    public int AwaitingCheckIn => Registered;

    public bool HasEligiblePlayers => EligiblePlayers > 0;
}
