using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using DevOpsBoard.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevOpsBoard.Infrastructure.Authorization;

namespace DevOpsBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DevOpsBoard"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se ha configurado la cadena de conexión 'DevOpsBoard'."
            );
        }

        services.AddDbContext<DevOpsBoardDbContext>(
            options =>
                options.UseNpgsql(connectionString)
        );

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<DevOpsBoardDbContext>();

        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<ITeamService, TeamService>();

        services.AddScoped<
            ITeamMemberRepository,
            TeamMemberRepository
        >();

        services.AddScoped<
            IUserRepository,
            UserRepository
        >();

        services.AddScoped<
            ITeamMemberService,
            TeamMemberService
        >();

        services.AddScoped<
            ITeamAuthorizationService,
            TeamAuthorizationService
        >();

        services.AddScoped<
            IProjectRepository,
            ProjectRepository
        >();

        services.AddScoped<
            IProjectService,
            ProjectService
        >();

        services.AddScoped<
            IProjectAuthorizationService,
            ProjectAuthorizationService
        >();

        services.AddScoped<
            IProjectMemberAuthorizationService,
            ProjectMemberAuthorizationService
        >();

        services.AddScoped<
            IProjectMemberRepository,
            ProjectMemberRepository
        >();

        services.AddScoped<
            IProjectMemberService,
            ProjectMemberService
        >();

        services.AddScoped<
            IIssueAuthorizationService,
            IssueAuthorizationService
        >();

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName
            )
        );

        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<IIssueService, IssueService>();

        return services;
    }
}