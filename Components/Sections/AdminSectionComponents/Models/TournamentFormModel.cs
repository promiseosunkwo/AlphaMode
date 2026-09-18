using System.ComponentModel.DataAnnotations;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Components.Sections.AdminSectionComponents.Models;

public class TournamentFormModel
{
    [Required(ErrorMessage = "Tournament name is required.")]
    [StringLength(150, ErrorMessage = "Tournament name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;


    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }


    [Required(ErrorMessage = "Please select a game.")]
    public Guid? GameId { get; set; }


    [Required(ErrorMessage = "Please select a tournament format.")]
    public TournamentFormat? Format { get; set; }


    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }


    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }


    public DateTime? RegistrationOpenDate { get; set; }


    public DateTime? RegistrationDeadline { get; set; }


    [Range(2, 10000, ErrorMessage = "Maximum players must be at least 2.")]
    public int MaxPlayers { get; set; } = 32;


    [Range(0, 10000000, ErrorMessage = "Entry fee cannot be negative.")]
    public decimal EntryFee { get; set; }


    [Range(1, 20, ErrorMessage = "Legs must be between 1 and 20.")]
    public int Legs { get; set; } = 1;


    [Range(0, 100, ErrorMessage = "Points for a win must be valid.")]
    public int PointsForWin { get; set; } = 3;


    [Range(0, 100, ErrorMessage = "Points for a draw must be valid.")]
    public int PointsForDraw { get; set; } = 1;


    [Range(0, 100, ErrorMessage = "Points for a loss must be valid.")]
    public int PointsForLoss { get; set; }


    [Url(ErrorMessage = "Please enter a valid image URL.")]
    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    public string? ImageUrl { get; set; }


    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
}