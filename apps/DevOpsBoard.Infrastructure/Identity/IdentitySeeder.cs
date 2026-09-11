using DevOpsBoard.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsBoard.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName)
                );

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(
                            error => error.Description
                        )
                    );

                    throw new InvalidOperationException(
                        $"No se pudo crear el rol '{roleName}': {errors}"
                    );
                }
            }
        }

        var adminEmail =
            configuration["Identity:AdminEmail"];

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        var adminUser =
            await userManager.FindByEmailAsync(
                adminEmail.Trim()
            );

        if (adminUser is null)
        {
            return;
        }

        if (!await userManager.IsInRoleAsync(
                adminUser,
                RoleNames.Admin))
        {
            var result = await userManager.AddToRoleAsync(
                adminUser,
                RoleNames.Admin
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(
                        error => error.Description
                    )
                );

                throw new InvalidOperationException(
                    $"No se pudo asignar el rol ADMIN: {errors}"
                );
            }
        }
    }
}