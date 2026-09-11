using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class ProjectMemberServiceTests
{
    private const string OwnerId =
        "owner-user-id";

    private const string ManagerId =
        "manager-user-id";

    private const string DeveloperId =
        "developer-user-id";

    private const string ViewerId =
        "viewer-user-id";

    private static readonly Guid ProjectId =
        Guid.NewGuid();

    [Fact]
    public async Task AddAsync_ShouldAddMember_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(true);

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddProjectMemberRequest(
            DeveloperId,
            "Developer"
        );

        var result = await service.AddAsync(
            ProjectId,
            request,
            OwnerId
        );

        Assert.Equal(
            DeveloperId,
            result.UserId
        );

        Assert.Equal(
            "Developer",
            result.DisplayName
        );

        Assert.Equal(
            "developer@devopsboard.local",
            result.Email
        );

        Assert.Equal(
            "Developer",
            result.Role
        );

        Assert.Single(
            memberRepository.Members
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(false);

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddProjectMemberRequest(
            DeveloperId,
            "Developer"
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.AddAsync(
                ProjectId,
                request,
                "unauthorized-user"
            )
        );

        Assert.Empty(
            memberRepository.Members
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowConflict_WhenMemberAlreadyExists()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(true);

        var existingMember = new ProjectMember(
            ProjectId,
            DeveloperId,
            ProjectRole.Developer
        );

        await memberRepository.AddAsync(
            existingMember
        );

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddProjectMemberRequest(
            DeveloperId,
            "Developer"
        );

        await Assert.ThrowsAsync<ConflictException>(
            () => service.AddAsync(
                ProjectId,
                request,
                OwnerId
            )
        );
    }

    [Fact]
    public async Task AddAsync_ShouldRejectInvalidRole()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(true);

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request = new AddProjectMemberRequest(
            DeveloperId,
            "Administrator"
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.AddAsync(
                ProjectId,
                request,
                OwnerId
            )
        );
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldChangeRole_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(true);

        var existingMember = new ProjectMember(
            ProjectId,
            DeveloperId,
            ProjectRole.Developer
        );

        await memberRepository.AddAsync(
            existingMember
        );

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request =
            new UpdateProjectMemberRoleRequest(
                "Manager"
            );

        var result = await service.UpdateRoleAsync(
            ProjectId,
            DeveloperId,
            request,
            OwnerId
        );

        Assert.Equal(
            "Manager",
            result.Role
        );

        Assert.Equal(
            ProjectRole.Manager,
            existingMember.Role
        );
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(DeveloperId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(false);

        var existingMember = new ProjectMember(
            ProjectId,
            DeveloperId,
            ProjectRole.Developer
        );

        await memberRepository.AddAsync(
            existingMember
        );

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        var request =
            new UpdateProjectMemberRoleRequest(
                "Manager"
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.UpdateRoleAsync(
                ProjectId,
                DeveloperId,
                request,
                "unauthorized-user"
            )
        );

        Assert.Equal(
            ProjectRole.Developer,
            existingMember.Role
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveMember_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(ViewerId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(true);

        var existingMember = new ProjectMember(
            ProjectId,
            ViewerId,
            ProjectRole.Viewer
        );

        await memberRepository.AddAsync(
            existingMember
        );

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        await service.RemoveAsync(
            ProjectId,
            ViewerId,
            OwnerId
        );

        Assert.Empty(
            memberRepository.Members
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var memberRepository =
            new FakeProjectMemberRepository();

        var userRepository =
            new FakeUserRepository(
                CreateUser(ViewerId)
            );

        var authorizationService =
            new FakeProjectMemberAuthorizationService(false);

        var existingMember = new ProjectMember(
            ProjectId,
            ViewerId,
            ProjectRole.Viewer
        );

        await memberRepository.AddAsync(
            existingMember
        );

        var service = new ProjectMemberService(
            projectRepository,
            memberRepository,
            userRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.RemoveAsync(
                ProjectId,
                ViewerId,
                "unauthorized-user"
            )
        );

        Assert.Single(
            memberRepository.Members
        );
    }

    private static UserSummaryDto CreateUser(
        string userId)
    {
        var email = userId switch
        {
            ManagerId =>
                "manager@devopsboard.local",

            DeveloperId =>
                "developer@devopsboard.local",

            ViewerId =>
                "viewer@devopsboard.local",

            _ =>
                "owner@devopsboard.local"
        };

        var displayName = userId switch
        {
            ManagerId => "Manager",

            DeveloperId => "Developer",

            ViewerId => "Viewer",

            _ => "Owner"
        };

        return new UserSummaryDto(
            userId,
            displayName,
            email
        );
    }

    private sealed class FakeProjectRepository
        : IProjectRepository
    {
        private readonly Project _project =
            new(
                "DevOpsBoard API",
                "DBAPI",
                OwnerId
            );

        public Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                id == ProjectId
                    ? _project
                    : null
            );
        }

        public Task<Project?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                id == ProjectId
                    ? _project
                    : null
            );
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Project>
            >(
                [_project]
            );
        }

        public Task<bool> ExistsByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                string.Equals(
                    key,
                    _project.Key,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(Project project)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjectMemberRepository
        : IProjectMemberRepository
    {
        public List<ProjectMember> Members { get; } = [];

        public Task<ProjectMember?> GetAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Members.FirstOrDefault(
                    member =>
                        member.ProjectId == projectId &&
                        member.UserId == userId
                )
            );
        }

        public Task<
            IReadOnlyList<ProjectMemberReadModel>
        > GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = Members
                .Where(
                    member =>
                        member.ProjectId == projectId
                )
                .Select(
                    member =>
                        new ProjectMemberReadModel(
                            member.UserId,
                            "Developer",
                            "developer@devopsboard.local",
                            member.Role.ToString(),
                            member.JoinedAt
                        )
                )
                .ToList();

            return Task.FromResult<
                IReadOnlyList<ProjectMemberReadModel>
            >(result);
        }

        public Task AddAsync(
            ProjectMember member,
            CancellationToken cancellationToken = default)
        {
            Members.Add(member);
            return Task.CompletedTask;
        }

        public void Remove(ProjectMember member)
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

    private sealed class FakeProjectMemberAuthorizationService
        : IProjectMemberAuthorizationService
    {
        private readonly bool _canManage;

        public FakeProjectMemberAuthorizationService(
            bool canManage)
        {
            _canManage = canManage;
        }

        public Task<bool> CanManageMembersAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canManage
            );
        }
    }
}