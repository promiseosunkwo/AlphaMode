using AlphaMode.Application.Common;
using AlphaMode.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AlphaMode.Infrastructure.Identity;

public class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // -------------------------
        // 1. CREATE ROLES
        // -------------------------
        string[] roles =
        {
            Roles.Admin,
            Roles.Player,
            Roles.TournamentManager
        };

        foreach (var role in roles)
        {
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // -------------------------
        // 2. CREATE DEFAULT ADMIN
        // -------------------------
        var adminEmail = "admin@alphamode.com";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                GamerTag = "AlphaAdmin"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }
}