
namespace AlphaMode.Application.Players.DTOs
{
    public class PlayerDto
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        // Identity information

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        // Player information

        public string GamerTag { get; set; } = string.Empty;

        public int RankingPoints { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Draws { get; set; }

        public bool IsActive { get; set; }

        // Calculated

        public int TournamentCount { get; set; }

        public int MatchesPlayed => Wins + Losses + Draws;

        public DateTime JoinedDate { get; set; }

        public decimal WinRate => MatchesPlayed == 0 ? 0 : Math.Round((decimal)Wins / MatchesPlayed * 100,1);

        public string FullName =>$"{FirstName} {LastName}".Trim();
    }
}
