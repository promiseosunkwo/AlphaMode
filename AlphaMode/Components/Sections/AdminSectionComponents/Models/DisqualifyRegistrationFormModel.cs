using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Components.Sections.AdminSectionComponents.Models;

public class DisqualifyRegistrationFormModel
{
    [Required(ErrorMessage = "A disqualification reason is required.")]
    [StringLength(
        500,
        MinimumLength = 5,
        ErrorMessage = "The reason must be between 5 and 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}