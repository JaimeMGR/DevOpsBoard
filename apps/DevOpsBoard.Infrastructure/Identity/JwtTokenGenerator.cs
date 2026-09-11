using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevOpsBoard.Infrastructure.Identity;

public class JwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenGenerator(
        IOptions<JwtOptions> options,
        UserManager<ApplicationUser> userManager)
    {
        _options = options.Value;
        _userManager = userManager;
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id
            ),
            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty
            ),
            new(
                ClaimTypes.Name,
                user.DisplayName
            ),
            new(
                ClaimTypes.NameIdentifier,
                user.Id
            )
        };

        claims.AddRange(
            roles.Select(role =>
                new Claim(
                    ClaimTypes.Role,
                    role
                )
            )
        );

        var expiresAt = DateTime.UtcNow.AddMinutes(
            _options.ExpirationMinutes
        );

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt
        );
    }
}