using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Components.Sections.AdminSectionComponents.Models;

public class PlayerFormModel
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;


    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;


    [Required(ErrorMessage = "Gamer tag is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Gamer tag must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Gamer tag can only contain letters, numbers, underscores and hyphens.")]
    public string GamerTag { get; set; } = string.Empty;
}