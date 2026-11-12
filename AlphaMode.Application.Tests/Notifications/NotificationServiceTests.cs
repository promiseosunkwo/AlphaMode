using AlphaMode.Application.Notifications.DTOs;
using AlphaMode.Application.Notifications.Services;
using AlphaMode.Application.Tests.Support;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Tests.Notifications;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task SendToActivePlayers_ExcludesInactivePlayers()
    {
        await using var db = new TestApplicationDbContext();
        var active = TestData.Player("Active"); var inactive = TestData.Player("Inactive", false);
        db.Players.AddRange(active, inactive); await db.SaveChangesAsync();
        var result = await new NotificationService(db).SendAsync(new() { Title = "Update", Message = "Ready", Audience = NotificationAudience.ActivePlayers });
        Assert.True(result.Succeeded); Assert.Equal(1, result.Value); Assert.Equal(active.UserId, Assert.Single(db.Notifications).UserId);
    }

    [Fact]
    public async Task TournamentAudience_OnlyIncludesEligibleParticipants()
    {
        await using var db = new TestApplicationDbContext();
        var game = TestData.Game(); var tournament = TestData.Tournament(game); var included = TestData.Player("Included"); var withdrawn = TestData.Player("Withdrawn"); var outsider = TestData.Player("Outsider");
        db.AddRange(game, tournament, included, withdrawn, outsider,
            new AlphaMode.Domain.Entities.TournamentRegistration { Id = Guid.NewGuid(), TournamentId = tournament.Id, Tournament = tournament, PlayerId = included.Id, Player = included, Status = TournamentRegistrationStatus.Registered },
            new AlphaMode.Domain.Entities.TournamentRegistration { Id = Guid.NewGuid(), TournamentId = tournament.Id, Tournament = tournament, PlayerId = withdrawn.Id, Player = withdrawn, Status = TournamentRegistrationStatus.Withdrawn });
        await db.SaveChangesAsync();
        var result = await new NotificationService(db).SendAsync(new() { Title = "Fixture", Message = "Published", Audience = NotificationAudience.TournamentParticipants, TournamentId = tournament.Id });
        Assert.True(result.Succeeded); Assert.Equal(included.UserId, Assert.Single(db.Notifications).UserId);
    }

    [Fact]
    public async Task MarkRead_CannotChangeAnotherUsersNotification()
    {
        await using var db = new TestApplicationDbContext();
        var item = new Notification { Id = Guid.NewGuid(), UserId = "owner", Title = "Private", Message = "Message" };
        db.Notifications.Add(item); await db.SaveChangesAsync();
        var result = await new NotificationService(db).MarkReadAsync(item.Id, "other-user");
        Assert.False(result.Succeeded); Assert.False(item.IsRead);
    }

    [Fact]
    public async Task MarkAllRead_OnlyUpdatesCurrentUsersInbox()
    {
        await using var db = new TestApplicationDbContext();
        db.Notifications.AddRange(new Notification { Id = Guid.NewGuid(), UserId = "one", Title = "A", Message = "A" }, new Notification { Id = Guid.NewGuid(), UserId = "two", Title = "B", Message = "B" });
        await db.SaveChangesAsync();
        await new NotificationService(db).MarkAllReadAsync("one");
        Assert.True(db.Notifications.Single(x => x.UserId == "one").IsRead); Assert.False(db.Notifications.Single(x => x.UserId == "two").IsRead);
    }

    [Fact]
    public async Task EmptyAudience_ReturnsFailureWithoutCreatingRows()
    {
        await using var db = new TestApplicationDbContext();
        var result = await new NotificationService(db).SendAsync(new() { Title = "None", Message = "None", Audience = NotificationAudience.TournamentParticipants, TournamentId = Guid.NewGuid() });
        Assert.False(result.Succeeded); Assert.Empty(db.Notifications);
    }
}
