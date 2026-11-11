using System.ComponentModel.DataAnnotations;


namespace AlphaMode.Application.TournamentMatches.Requests;

public class ScheduleMatchRequest
{
    public Guid MatchId { get; set; }

    [Required]
    public DateTime? ScheduledAt { get; set; }
}
