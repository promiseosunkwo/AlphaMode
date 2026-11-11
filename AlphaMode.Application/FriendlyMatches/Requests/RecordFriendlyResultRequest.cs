using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.FriendlyMatches.Requests;

public sealed class RecordFriendlyResultRequest
{
    [Required] public Guid MatchId { get; set; }
    [Range(0, 999)] public int PlayerOneScore { get; set; }
    [Range(0, 999)] public int PlayerTwoScore { get; set; }
}

