using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.TournamentRegistrations.Requests;

public class DisqualifyRegistrationRequest
{
    public Guid RegistrationId { get; set; }

    [Required(ErrorMessage = "A disqualification reason is required.")]
    [StringLength(
        500,
        MinimumLength = 5,
        ErrorMessage = "The reason must be between 5 and 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}
