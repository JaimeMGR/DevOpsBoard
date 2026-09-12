using DevOpsBoard.Application.Security;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsBoard.Tests.Infrastructure;

public sealed class ApiTestHelper
{
    private readonly ApiFactory _factory;

    public ApiTestHelper(ApiFactory factory)
    {
        _factory = factory;
    }

    public async Task<TestUser> CreateUserAsync(
        string email,
        string displayName)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser =
            await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return new TestUser(
                existingUser.Id,
                email,
                existingUser.DisplayName
            );
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            NormalizedUserName = email.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            DisplayName = displayName,
            IsActive = true
        };

        var result =
            await userManager.CreateAsync(
                user,
                "Test1234!"
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
                $"No se pudo crear el usuario '{email}': {errors}"
            );
        }

        return new TestUser(
            user.Id,
            email,
            displayName
        );
    }

    public async Task<string> GenerateTokenAsync(
        string userId)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var tokenGenerator =
            scope.ServiceProvider
                .GetRequiredService<JwtTokenGenerator>();

        var user =
            await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"No existe el usuario '{userId}'."
            );
        }

        var result =
            await tokenGenerator.GenerateAsync(user);

        return result.Token;
    }

    public async Task AddRoleAsync(
        string userId,
        string role)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"No existe el usuario '{userId}'."
            );
        }

        if (await userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result =
            await userManager.AddToRoleAsync(
                user,
                role
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
                $"No se pudo asignar el rol '{role}' al usuario '{user.Email}': {errors}"
            );
        }
    }

    public async Task<ApplicationUser?> GetUserAsync(
        string userId)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        return await userManager.FindByIdAsync(
            userId
        );
    }


    public async Task<DevOpsBoardDbContext>
        CreateDbContextAsync()
    {
        var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<DevOpsBoardDbContext>();

        return await Task.FromResult(
            dbContext
        );
    }

    public sealed record TestUser(
    string Id,
    string Email,
    string DisplayName
);
}

