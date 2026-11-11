using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.Notifications.DTOs;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class SendNotificationRequest
{
    [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
    [Required, MaxLength(1000)] public string Message { get; set; } = string.Empty;
    [Required] public NotificationAudience Audience { get; set; } = NotificationAudience.AllPlayers;
    public Guid? TournamentId { get; set; }
}

public enum NotificationAudience { AllPlayers, ActivePlayers, TournamentParticipants }

public sealed class NotificationBroadcastDto
{
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public int RecipientCount { get; init; }
}

