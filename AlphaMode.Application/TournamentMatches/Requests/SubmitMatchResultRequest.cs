using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.TournamentMatches.Requests;

public sealed class SubmitMatchResultRequest
{
    public Guid MatchId { get; set; }

    [Required(ErrorMessage = "Enter Player 1's score.")]
    [Range(0,999, ErrorMessage = "Score must be between 0 and 999.")]
    public int? PlayerOneScore { get; set; }

    [Required(ErrorMessage = "Enter Player 2's score.")]
    [Range(0,999, ErrorMessage = "Score must be between 0 and 999.")]
    public int? PlayerTwoScore { get; set; }

    public bool WentToExtraTime { get; set; }

    public int? PlayerOnePenaltyScore { get; set; }

    public int? PlayerTwoPenaltyScore { get; set; }
}
