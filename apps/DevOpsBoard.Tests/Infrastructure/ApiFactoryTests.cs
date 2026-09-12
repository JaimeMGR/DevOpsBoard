using System.Net;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class ApiFactoryTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ApiFactoryTests(
        ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_ShouldStart()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/health"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }
}