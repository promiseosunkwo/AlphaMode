namespace AlphaMode.ViewModels;

// UI-only view models keep the admin pages independent from persistence concerns.
public sealed class AdminTournamentItem { public int Id { get; set; } public string Name { get; set; } = ""; public string Game { get; set; } = ""; public DateTime StartsOn { get; set; } = DateTime.Today; public int Capacity { get; set; } public int Registered { get; set; } public string Status { get; set; } = "Draft"; }
public sealed class AdminPlayerItem { public int Id { get; set; } public string GamerTag { get; set; } = ""; public string Email { get; set; } = ""; public string Status { get; set; } = "Active"; public int Ranking { get; set; } public int Matches { get; set; } }
public sealed class AdminGameItem { public int Id { get; set; } public string Name { get; set; } = ""; public string Genre { get; set; } = ""; public string Platform { get; set; } = ""; public bool Enabled { get; set; } = true; }
public sealed class AdminMatchItem { public int Id { get; set; } public string Tournament { get; set; } = ""; public string PlayerOne { get; set; } = ""; public string PlayerTwo { get; set; } = ""; public DateTime ScheduledFor { get; set; } = DateTime.Now.AddDays(1); public string Status { get; set; } = "Scheduled"; }
public sealed class AdminNotificationItem { public int Id { get; set; } public string Title { get; set; } = ""; public string Message { get; set; } = ""; public string Audience { get; set; } = "All players"; public string Channel { get; set; } = "In-app"; public DateTime CreatedAt { get; set; } = DateTime.Now; }
