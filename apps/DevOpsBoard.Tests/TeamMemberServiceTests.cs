using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class TeamMemberServiceTests
{
    private static readonly Team TestTeam =
    new Team(
        "DevOps",
        "Equipo de infraestructura",
        ActingUserId
    );

    private static readonly Guid TeamId = TestTeam.Id;


    private const string ActingUserId =
        "admin-user-id";

    private const string MemberUserId =
        "developer-user-id";

    private static UserSummaryDto CreateUser(
        string userId = MemberUserId)
    {
        return new UserSummaryDto(
            userId,
            "Developer",
            "developer@devopsboard.local"
        );
    }

    [Fact]
    public async Task AddAsync_ShouldAddMember_WhenAuthorized()
    {
        var teamRepository = new FakeTeamRepository();
        var memberRepository = new FakeTeamMemberRepository();
        var userRepository = new FakeUserRepository(
            CreateUser()
        );
        var authorizationService =
            new FakeTeamAuthorizationService(true);

        var service = new TeamMemberService(
            teamRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddTeamMemberRequest(
            MemberUserId,
            "Member"
        );

        var result = await service.AddAsync(
            TeamId,
            request,
            ActingUserId
        );

        Assert.Equal(MemberUserId, result.UserId);
        Assert.Equal("Developer", result.DisplayName);
        Assert.Equal(
            "developer@devopsboard.local",
            result.Email
        );
        Assert.Equal("Member", result.Role);

        Assert.Single(memberRepository.Members);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var teamRepository = new FakeTeamRepository();
        var memberRepository = new FakeTeamMemberRepository();
        var userRepository = new FakeUserRepository(
            CreateUser()
        );
        var authorizationService =
            new FakeTeamAuthorizationService(false);

        var service = new TeamMemberService(
            teamRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddTeamMemberRequest(
            MemberUserId,
            "Member"
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.AddAsync(
                TeamId,
                request,
                ActingUserId
            )
        );

        Assert.Empty(memberRepository.Members);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowConflict_WhenMemberAlreadyExists()
    {
        var teamRepository = new FakeTeamRepository();
        var memberRepository = new FakeTeamMemberRepository();
        var userRepository = new FakeUserRepository(
            CreateUser()
        );
        var authorizationService =
            new FakeTeamAuthorizationService(true);

        var existingMember = new TeamMember(
            TeamId,
            MemberUserId,
            TeamRole.Member
        );

        await memberRepository.AddAsync(existingMember);

        var service = new TeamMemberService(
            teamRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddTeamMemberRequest(
            MemberUserId,
            "Member"
        );

        await Assert.ThrowsAsync<ConflictException>(
            () => service.AddAsync(
                TeamId,
                request,
                ActingUserId
            )
        );
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldChangeRole_WhenAuthorized()
    {
        var teamRepository = new FakeTeamRepository();
        var memberRepository = new FakeTeamMemberRepository();
        var userRepository = new FakeUserRepository(
            CreateUser()
        );
        var authorizationService =
            new FakeTeamAuthorizationService(true);

        var existingMember = new TeamMember(
            TeamId,
            MemberUserId,
            TeamRole.Member
        );

        await memberRepository.AddAsync(existingMember);

        var service = new TeamMemberService(
            teamRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateTeamMemberRoleRequest(
            "Lead"
        );

        var result = await service.UpdateRoleAsync(
            TeamId,
            MemberUserId,
            request,
            ActingUserId
        );

        Assert.Equal("Lead", result.Role);
        Assert.Equal(
            TeamRole.Lead,
            existingMember.Role
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveMember_WhenAuthorized()
    {
        var teamRepository = new FakeTeamRepository();
        var memberRepository = new FakeTeamMemberRepository();
        var userRepository = new FakeUserRepository(
            CreateUser()
        );
        var authorizationService =
            new FakeTeamAuthorizationService(true);

        var existingMember = new TeamMember(
            TeamId,
            MemberUserId,
            TeamRole.Lead
        );

        await memberRepository.AddAsync(existingMember);

        var service = new TeamMemberService(
            teamRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        await service.RemoveAsync(
            TeamId,
            MemberUserId,
            ActingUserId
        );

        Assert.Empty(memberRepository.Members);
    }

    private sealed class FakeTeamRepository
        : ITeamRepository
    {
        private readonly List<Team> _teams =
        [
            TestTeam
        ];

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

    private sealed class FakeTeamMemberRepository
        : ITeamMemberRepository
    {
        public List<TeamMember> Members { get; } = [];

        public Task<TeamMember?> GetAsync(
            Guid teamId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Members.FirstOrDefault(
                    member =>
                        member.TeamId == teamId &&
                        member.UserId == userId
                )
            );
        }

        public Task<IReadOnlyList<TeamMemberReadModel>>
            GetByTeamIdAsync(
                Guid teamId,
                CancellationToken cancellationToken = default)
        {
            var result = Members
                .Where(member => member.TeamId == teamId)
                .Select(member =>
                    new TeamMemberReadModel(
                        member.UserId,
                        "Developer",
                        "developer@devopsboard.local",
                        member.Role.ToString(),
                        member.JoinedAt
                    )
                )
                .ToList();

            return Task.FromResult<
                IReadOnlyList<TeamMemberReadModel>
            >(result);
        }

        public Task AddAsync(
            TeamMember member,
            CancellationToken cancellationToken = default)
        {
            Members.Add(member);
            return Task.CompletedTask;
        }

        public void Remove(TeamMember member)
        {
            Members.Remove(member);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        private readonly UserSummaryDto _user;

        public FakeUserRepository(
            UserSummaryDto user)
        {
            _user = user;
        }

        public Task<bool> ExistsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                userId == _user.Id
            );
        }

        public Task<UserSummaryDto?> GetSummaryByIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                userId == _user.Id
                    ? _user
                    : null
            );
        }
    }

    private sealed class FakeTeamAuthorizationService
        : ITeamAuthorizationService
    {
        private readonly bool _canManage;

        public FakeTeamAuthorizationService(
            bool canManage)
        {
            _canManage = canManage;
        }

        public Task<bool> CanManageMembersAsync(
            Guid teamId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_canManage);
        }
    }
}