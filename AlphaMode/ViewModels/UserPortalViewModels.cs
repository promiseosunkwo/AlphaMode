namespace AlphaMode.ViewModels;

// UI-focused records keep dashboard pages independent from persistence entities.
public sealed record PlayerProfileViewModel(string DisplayName, string GamerTag, string Email, string Initials, int Level, int LevelProgress, DateTime MemberSince, string FavouriteGame, string Region);
public sealed record PlayerStatsViewModel(int Matches, int Wins, int Losses, int WinRate, int Points, int CurrentRank, int BestRank, int Trophies);
public sealed record PlayerMatchViewModel(int Id, string Opponent, string OpponentInitials, string Game, string Tournament, DateTime StartsAt, string Status, int? PlayerScore = null, int? OpponentScore = null);
public sealed record PlayerTournamentViewModel(int Id, string Name, string Game, DateTime StartsAt, int RegisteredPlayers, int Capacity, decimal PrizePool, string Status, bool IsJoined = false);
public sealed record PlayerFixtureViewModel(int Round, string PlayerOne, string PlayerTwo, DateTime ScheduledAt, string Station, string Status, bool IsCurrentUserFixture = false);
public sealed record PlayerNotificationViewModel(int Id, string Type, string Title, string Message, DateTime CreatedAt, bool IsRead, string? Link = null);
public sealed record RankingEntryViewModel(int Rank, string Player, string Initials, string Game, int Wins, int Points, string Trend, bool IsCurrentUser = false);

