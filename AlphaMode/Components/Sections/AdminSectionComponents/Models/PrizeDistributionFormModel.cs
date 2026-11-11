using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Components.Sections.AdminSectionComponents.Models
{
    public class PrizeDistributionFormModel
    {
        public Guid? Id { get; set; }
        public int Position { get; set; }

        [Range(
            0.01,
            100000000000,
            ErrorMessage = "Prize amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
