using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.PlayerTournamentRegistrations.DTOs;

public class TournamentRegistrationAvailabilityDto
{
    public Guid TournamentId { get; set; }

    public Guid PlayerId { get; set; }

    public Guid? RegistrationId { get; set; }

    public TournamentStatus TournamentStatus { get; set; }

    public TournamentRegistrationStatus? RegistrationStatus { get; set; }

    public int RegisteredPlayers { get; set; }

    public int MaxPlayers { get; set; }

    public bool IsFull { get; set; }

    public bool CanRegister { get; set; }

    public bool CanCheckIn { get; set; }

    public bool CanWithdraw { get; set; }

    public string Message { get; set; } = string.Empty;
}