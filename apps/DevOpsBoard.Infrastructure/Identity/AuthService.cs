using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DevOpsBoard.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        JwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException(
                "El email es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException(
                "La contraseña es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ValidationException(
                "El nombre visible es obligatorio."
            );
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(
            email
        );

        if (existingUser is not null)
        {
            throw new ConflictException(
                "Ya existe un usuario con ese email."
            );
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password
        );

        if (!result.Succeeded)
        {
            var errors = string.Join(
                " ",
                result.Errors.Select(error => error.Description)
            );

            throw new ValidationException(errors);
        }

        var token = await _tokenGenerator.GenerateAsync(
            user
        );

        return new AuthResponse(
            token.Token,
            token.ExpiresAt
        );
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException(
                "Email y contraseña son obligatorios."
            );
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(
            email
        );

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Credenciales no válidas."
            );
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password
            );

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Credenciales no válidas."
            );
        }

        var token = await _tokenGenerator.GenerateAsync(
            user
        );

        return new AuthResponse(
            token.Token,
            token.ExpiresAt
        );
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.ToList()
        );
    }
}