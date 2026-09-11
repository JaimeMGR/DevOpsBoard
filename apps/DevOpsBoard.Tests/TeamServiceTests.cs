using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class TeamServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateTeam()
    {
        var repository = new FakeTeamRepository();

        var service = new TeamService(repository);

        var request = new CreateTeamRequest(
            "Backend",
            "Equipo encargado del backend"
        );

        var result = await service.CreateAsync(
            request,
            "test-user-id"
        );

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Backend", result.Name);
        Assert.Equal(
            "Equipo encargado del backend",
            result.Description
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateName()
    {
        var repository = new FakeTeamRepository();

        await repository.AddAsync(
            new Team(
                "Backend",
                null,
                "test-user-id"
            )
        );

        var service = new TeamService(repository);

        var request = new CreateTeamRequest(
            "Backend",
            null
        );

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                request,
                "test-user-id"
            )
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEmptyName()
    {
        var repository = new FakeTeamRepository();

        var service = new TeamService(repository);

        var request = new CreateTeamRequest(
            "",
            "Descripción"
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(
                request,
                "test-user-id"
            )
        );
    }

    private sealed class FakeTeamRepository : ITeamRepository
    {
        private readonly List<Team> _teams = [];

        public Task<Team?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _teams.FirstOrDefault(
                    team => team.Id == id
                )
            );
        }

        public Task<IReadOnlyList<Team>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Team>>(
                _teams.ToList()
            );
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _teams.Any(
                    team => team.Name == name
                )
            );
        }

        public Task AddAsync(
            Team team,
            CancellationToken cancellationToken = default)
        {
            _teams.Add(team);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}