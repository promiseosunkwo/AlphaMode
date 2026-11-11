using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.TournamentMatches.Requests;
public class ForfeitMatchRequest
{
    public Guid MatchId { get; set; }

    public MatchResultType ResultType { get; set; }

    public MatchForfeitReason Reason { get; set; }

    public Guid? ForfeitingPlayerId { get; set; }

    public string? Notes { get; set; }
}