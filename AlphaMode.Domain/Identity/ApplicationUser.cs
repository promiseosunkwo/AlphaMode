using AlphaMode.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AlphaMode.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string GamerTag { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public virtual Player? Player { get; set; }
}