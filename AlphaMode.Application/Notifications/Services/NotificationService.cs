using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Notifications.DTOs;
using AlphaMode.Application.Notifications.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Notifications.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    public NotificationService(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<NotificationDto>>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Result<IReadOnlyList<NotificationDto>>.Failure("A user is required.");
        var items = await _context.Notifications.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedDate).Take(100)
            .Select(x => new NotificationDto { Id = x.Id, Title = x.Title, Message = x.Message, IsRead = x.IsRead, CreatedAt = x.CreatedDate })
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<NotificationDto>>.Success(items);
    }

    public async Task<Result> MarkReadAsync(Guid notificationId, string userId, CancellationToken cancellationToken = default)
    {
        // UserId is part of the predicate so one player can never mutate another player's inbox.
        var item = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);
        if (item is null) return Result.Failure("Notification was not found.");
        if (!item.IsRead) { item.IsRead = true; item.ReadAt = DateTime.UtcNow; await _context.SaveChangesAsync(cancellationToken); }
        return Result.Success("Notification marked as read.");
    }

    public async Task<Result> MarkAllReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.Notifications.Where(x => x.UserId == userId && !x.IsRead).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var item in items) { item.IsRead = true; item.ReadAt = now; }
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("All notifications marked as read.");
    }

    public async Task<Result<int>> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message)) return Result<int>.Failure("Title and message are required.");
        IQueryable<AlphaMode.Domain.Entities.Player> recipients = _context.Players.AsNoTracking();
        recipients = request.Audience switch
        {
            NotificationAudience.ActivePlayers => recipients.Where(x => x.IsActive),
            NotificationAudience.TournamentParticipants when request.TournamentId.HasValue => recipients.Where(x => x.Registrations.Any(r => r.TournamentId == request.TournamentId && r.Status != TournamentRegistrationStatus.Withdrawn && r.Status != TournamentRegistrationStatus.Cancelled && r.Status != TournamentRegistrationStatus.Disqualified)),
            NotificationAudience.TournamentParticipants => recipients.Where(_ => false),
            _ => recipients
        };
        var userIds = await recipients.Select(x => x.UserId).Distinct().ToListAsync(cancellationToken);
        if (userIds.Count == 0) return Result<int>.Failure("The selected audience has no recipients.");
        // One row per recipient keeps inbox reads independent and works with the existing schema.
        _context.Notifications.AddRange(userIds.Select(userId => new Notification { Id = Guid.NewGuid(), UserId = userId, Title = request.Title.Trim(), Message = request.Message.Trim() }));
        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(userIds.Count, $"Notification sent to {userIds.Count} player(s).");
    }

    public async Task<Result<IReadOnlyList<NotificationBroadcastDto>>> GetRecentBroadcastsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Notifications.AsNoTracking().OrderByDescending(x => x.CreatedDate).Take(500).ToListAsync(cancellationToken);
        // Existing storage is recipient-based, so identical rows created together are collapsed into one admin log entry.
        var broadcasts = items.GroupBy(x => new { x.Title, x.Message, Minute = new DateTime(x.CreatedDate.Year, x.CreatedDate.Month, x.CreatedDate.Day, x.CreatedDate.Hour, x.CreatedDate.Minute, 0, DateTimeKind.Utc) })
            .OrderByDescending(x => x.Key.Minute).Take(30)
            .Select(x => new NotificationBroadcastDto { Title = x.Key.Title, Message = x.Key.Message, SentAt = x.Max(y => y.CreatedDate), RecipientCount = x.Count() }).ToList();
        return Result<IReadOnlyList<NotificationBroadcastDto>>.Success(broadcasts);
    }
}

