namespace AlphaMode.Application.Games.DTOs;

public class GameDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public int TournamentCount { get; set; }

    public DateTime CreatedDate { get; set; }
}
