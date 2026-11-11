namespace AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;

public class AdminTournamentRegisteredParticipantsDto
{
    public AdminTournamentReadinessDto Tournament { get; set; } = new();

    public IReadOnlyList<AdminTournamentRegisteredParticipantDto> Participants { get; set; } = [];
}
