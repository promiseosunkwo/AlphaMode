using AlphaMode.ViewModels;

namespace AlphaMode.Data;

// Centralized preview data can be replaced by query services without rewriting page markup.
public static class UserPortalSampleData
{
    public static readonly PlayerProfileViewModel Profile = new("Toyin Ade", "ToyinFC", "toyin@example.com", "TA", 18, 72, new DateTime(2025, 3, 14), "EA FC 26", "London, UK");
    public static readonly PlayerStatsViewModel Stats = new(42, 31, 11, 74, 2860, 12, 7, 4);
    public static readonly IReadOnlyList<PlayerMatchViewModel> Matches = new[] { new PlayerMatchViewModel(1,"ShadowX","SX","EA FC 26","Alpha Cup",DateTime.Today.AddDays(1).AddHours(19),"Upcoming"), new PlayerMatchViewModel(2,"NeoBlade","NB","Tekken 8","Elite Series",DateTime.Today.AddDays(3).AddHours(20),"Upcoming"), new PlayerMatchViewModel(3,"FatalKen","FK","Mortal Kombat","Mortal Clash",DateTime.Today.AddDays(-2).AddHours(18),"Won",3,1), new PlayerMatchViewModel(4,"KingJay","KJ","EA FC 26","Weekend League",DateTime.Today.AddDays(-5).AddHours(20),"Lost",1,2) };
    public static readonly IReadOnlyList<PlayerTournamentViewModel> Tournaments = new[] { new PlayerTournamentViewModel(1,"Alpha Cup","EA FC 26",DateTime.Today.AddDays(4),26,32,1000,"Registration open",true), new PlayerTournamentViewModel(2,"Mortal Clash","Mortal Kombat",DateTime.Today.AddDays(8),12,16,500,"Registration open"), new PlayerTournamentViewModel(3,"Tekken Masters","Tekken 8",DateTime.Today.AddDays(12),18,32,750,"Registration open"), new PlayerTournamentViewModel(4,"Night Ops","Call of Duty",DateTime.Today.AddDays(18),38,48,1500,"Coming soon") };
    public static readonly IReadOnlyList<PlayerFixtureViewModel> Fixtures = new[] { new PlayerFixtureViewModel(1,"ToyinFC","ShadowX",DateTime.Today.AddDays(1).AddHours(19),"Arena 2","Scheduled",true), new PlayerFixtureViewModel(1,"NeoBlade","KingJay",DateTime.Today.AddDays(1).AddHours(19),"Arena 3","Scheduled"), new PlayerFixtureViewModel(2,"Winner M1","Winner M2",DateTime.Today.AddDays(2).AddHours(20),"Main Stage","Awaiting players",true) };
    public static readonly IReadOnlyList<PlayerNotificationViewModel> Notifications = new[] { new PlayerNotificationViewModel(1,"match","Match confirmed","Your Alpha Cup match against ShadowX is confirmed.",DateTime.Now.AddMinutes(-18),false,"dashboard/fixtures"), new PlayerNotificationViewModel(2,"rank","Ranking update","You moved up 3 places to global rank #12.",DateTime.Now.AddHours(-3),false,"dashboard/ranking"), new PlayerNotificationViewModel(3,"tournament","Registration open","Registration for Tekken Masters is now open.",DateTime.Now.AddDays(-1),false,"dashboard/tournaments"), new PlayerNotificationViewModel(4,"result","Result recorded","Your 3–1 win against FatalKen has been recorded.",DateTime.Now.AddDays(-2),true,"dashboard/matches") };
    public static readonly IReadOnlyList<RankingEntryViewModel> Rankings = Enumerable.Range(1, 15).Select(i => new RankingEntryViewModel(i, i == 12 ? "ToyinFC" : $"AlphaPlayer{i}", i == 12 ? "TF" : $"P{i}", i % 2 == 0 ? "EA FC 26" : "Tekken 8", 44 - i, 3400 - i * 45, i % 3 == 0 ? "down" : "up", i == 12)).ToArray();
}

