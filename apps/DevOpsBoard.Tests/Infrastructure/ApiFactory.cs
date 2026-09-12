using System.Text;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace DevOpsBoard.Tests.Infrastructure;

public sealed class ApiFactory
    : WebApplicationFactory<Program>,
      IAsyncLifetime
{
    private const string JwtKey =
        "DevOpsBoardIntegrationTestsSecretKey_2026_09_12_ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private const string JwtIssuer =
        "DevOpsBoard.Tests";

    private const string JwtAudience =
        "DevOpsBoard.Tests.Client";

    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("devopsboard_api_tests")
            .WithUsername("devopsboard")
            .WithPassword("devopsboard")
            .Build();

    public string ConnectionString =>
        _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var db =
            CreateDbContext();

        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                var settings =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DevOpsBoard"] =
                            ConnectionString,

                        ["Jwt:Key"] =
                            JwtKey,

                        ["Jwt:Issuer"] =
                            JwtIssuer,

                        ["Jwt:Audience"] =
                            JwtAudience,

                        ["Jwt:ExpirationMinutes"] =
                            "60"
                    };

                configuration.AddInMemoryCollection(
                    settings
                );
            }
        );

        builder.ConfigureServices(
    services =>
    {
        services.Configure<JwtOptions>(
            options =>
            {
                options.Key =
                    JwtKey;

                options.Issuer =
                    JwtIssuer;

                options.Audience =
                    JwtAudience;

                options.ExpirationMinutes =
                    60;
            }
        );

        services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            JwtIssuer,

                        ValidAudience =
                            JwtAudience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    JwtKey
                                )
                            ),

                        ClockSkew =
                            TimeSpan.Zero
                    };
            }
        );

        services.Configure<
            Microsoft.AspNetCore.Authentication.AuthenticationOptions
        >(
            options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            }
        );
    }
);
    }

    private DevOpsBoardDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<DevOpsBoardDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        return new DevOpsBoardDbContext(options);
    }
}