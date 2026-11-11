using AlphaMode.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using FriendlyMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.FriendlyMatches.Requests;

public sealed class CreateFriendlyMatchRequest
{
    [Required] public Guid GameId { get; set; }
    [Required] public Guid PlayerOneId { get; set; }
    [Required] public Guid PlayerTwoId { get; set; }
    public FriendlyMatchType Type { get; set; } = FriendlyMatchType.Friendly;
    public DateTime? ScheduledAt { get; set; }
}

