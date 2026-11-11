using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Notifications.DTOs;

namespace AlphaMode.Application.Notifications.Interfaces;

public interface INotificationService
{
    Task<Result<IReadOnlyList<NotificationDto>>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> MarkReadAsync(Guid notificationId, string userId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllReadAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<int>> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<NotificationBroadcastDto>>> GetRecentBroadcastsAsync(CancellationToken cancellationToken = default);
}

