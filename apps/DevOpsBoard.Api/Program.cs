using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Api.Extensions;
using DevOpsBoard.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration
);

builder.Services.AddJwtAuthentication(
    builder.Configuration
);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

await IdentitySeeder.SeedAsync(
    app.Services,
    app.Configuration
);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}