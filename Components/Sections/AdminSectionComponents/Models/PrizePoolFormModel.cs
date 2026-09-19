using AlphaMode.Components.Sections.AdminSectionComponents.Models;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Application.PrizePools.Models;

public class PrizePoolFormModel
{
    [Required(ErrorMessage = "Please select a tournament.")]
    public Guid? TournamentId { get; set; }

    [Range(0.01, 100000000000, ErrorMessage = "Total prize must be greater than zero.")]
    public decimal TotalPrize { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a three-letter code.")]
    public string Currency { get; set; } = "NGN";

    public List<PrizeDistributionFormModel> Distributions{ get; set; } = [];
}