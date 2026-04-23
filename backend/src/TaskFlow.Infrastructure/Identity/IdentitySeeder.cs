using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureUserAsync(userManager, "admin@demo.com", "Admin@123", "Admin");
        await EnsureUserAsync(userManager, "user@demo.com", "User@123", "User");
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
