namespace AlphaMode.Application.Players.DTOs
{
    public class PlayerDetailsDto
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;


        // =========================================
        // ACCOUNT
        // =========================================

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public bool EmailConfirmed { get; set; }


        // =========================================
        // PLAYER
        // =========================================

        public string GamerTag { get; set; } = string.Empty;

        public int RankingPoints { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Draws { get; set; }

        public bool IsActive { get; set; }


        // =========================================
        // CALCULATED
        // =========================================

        public int MatchesPlayed => Wins + Losses + Draws;

        public int TournamentCount { get; set; }

        public decimal WinRate => MatchesPlayed == 0
                ? 0
                : Math.Round(
                    (decimal)Wins / MatchesPlayed * 100,
                    2);


        // =========================================
        // AUDIT
        // =========================================

        public DateTime JoinedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string FullName =>$"{FirstName} {LastName}".Trim();
    }
}
