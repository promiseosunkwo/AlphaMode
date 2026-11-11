using AlphaMode.Domain.Enums;
using FriendlyMatchType = AlphaMode.Domain.Enums.MatchType;

namespace AlphaMode.Application.FriendlyMatches.DTOs;

// A dedicated DTO prevents the UI from depending on tournament-only match fields.
public sealed class FriendlyMatchDto
{
    public Guid Id { get; init; }
    public Guid GameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public Guid PlayerOneId { get; init; }
    public string PlayerOneName { get; init; } = string.Empty;
    public Guid PlayerTwoId { get; init; }
    public string PlayerTwoName { get; init; } = string.Empty;
    public FriendlyMatchType Type { get; init; }
    public MatchStatus Status { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int? PlayerOneScore { get; init; }
    public int? PlayerTwoScore { get; init; }
    public Guid? WinnerId { get; init; }
    public bool IsRanked { get; init; }
}

