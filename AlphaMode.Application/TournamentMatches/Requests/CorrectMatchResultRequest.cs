using AlphaMode.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.TournamentMatches.Requests;

public class CorrectMatchResultRequest
{
    public Guid MatchId { get; set; }

    [Required(ErrorMessage = "Player one score is required.")]
    [Range(0, 999, ErrorMessage = "Score must be zero or greater.")]
    public int? PlayerOneScore { get; set; }

    [Required(ErrorMessage = "Player two score is required.")]
    [Range(0, 999, ErrorMessage = "Score must be zero or greater.")]
    public int? PlayerTwoScore { get; set; }

    [Required(ErrorMessage = "A reason for the correction is required.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Please provide a short reason for the correction.")]
    public string Reason { get; set; } = string.Empty;

    public bool WentToExtraTime { get; set; }

    public int? PlayerOnePenaltyScore { get; set; }

    public int? PlayerTwoPenaltyScore { get; set; }

}
