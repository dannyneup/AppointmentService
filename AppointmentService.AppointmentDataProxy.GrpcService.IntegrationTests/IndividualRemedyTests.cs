using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class IndividualRemedyTests(
    GrpcServiceTestFixture<Program> serviceTestFixture,
    KeycloakTestFixture keycloakTestFixture,
    ITestOutputHelper outputHelper)
    : IntegrationTestBase(serviceTestFixture, keycloakTestFixture, outputHelper)
{
    protected override string[] TokenScopes => [ Constants.Authorization.Scopes.Customer ];

    [Fact]
    public async Task Stream_WhenIndividualRemedyCreated_ShouldReturnIndividualRemedy()
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedy = new IndividualRemedy
        {
            Id = 456,
            Name = "John Doe",
        };

        await client.CreateAsync(new CreateIndividualRemedyRequest
        {
            IndividualRemedy = individualRemedy
        });
        var streamedIndividualRemedies = await ReadIndividualRemedyStreamAsync(client);
        Assert.Single(streamedIndividualRemedies, individualRemedy);
    }

    [Fact]
    public async Task Stream_WhenIndividualRemedyCreatedAndUpdated_ShouldContainUpdatedIndividualRemedy()
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);
        var individualRemedy = new IndividualRemedy
        {
            Id = 456,
            Name = "John Doe",
        };
        var updatedIndividualRemedy = new IndividualRemedy
        {
            Id = individualRemedy.Id,
            Name = "Sir John Doe"
        };

        await client.CreateAsync(new CreateIndividualRemedyRequest
        {
            IndividualRemedy = individualRemedy
        });
        await client.UpdateAsync(new UpdateIndividualRemedyRequest { IndividualRemedy = updatedIndividualRemedy });
        var streamedIndividualRemedies = await ReadIndividualRemedyStreamAsync(client);
        Assert.Single(streamedIndividualRemedies, updatedIndividualRemedy);
    }

    [Fact]
    public async Task Get_WhenIndividualRemedyCreated_ShouldReturnIndividualRemedy()
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedy = new IndividualRemedy
        {
            Id = 123,
            Name = "John Doe"
        };

        await client.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });

        var result = await client.GetAsync(new GetIndividualRemedyRequest { Id = individualRemedy.Id });

        Assert.Equivalent(result.IndividualRemedy, individualRemedy);
    }

    [Fact]
    public async Task Update_WhenIndividualRemedyDoesNotExists_ShouldThrowNotFound()
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var notExisting = new IndividualRemedy
        {
            Id = 123,
            Name = "Nobody",
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.UpdateAsync(new UpdateIndividualRemedyRequest { IndividualRemedy = notExisting }));

        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task Update_WhenIndividualRemedyDeleted_ShouldReturnNotFound()
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedy = new IndividualRemedy
        {
            Id = 123,
            Name = "John Doe"
        };

        await client.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });

        await client.DeleteAsync(new DeleteIndividualRemedyRequest { Id = individualRemedy.Id });

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetAsync(new GetIndividualRemedyRequest { Id = individualRemedy.Id }));

        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
    public async Task Stream_WhenFilteredById_ShouldReturnFilteredIndividualRemedies(Int32Filter filter,
        int id,
        bool expected)
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedy = new IndividualRemedy
        {
            Id = id,
            Name = "John Doe"
        };
        await client.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });
        var streamedIndividualRemedies =
            await ReadIndividualRemedyStreamAsync(client, new IndividualRemedyFilter { Id = filter });
        var expectedIndividualRemedies = expected ? [individualRemedy] : new List<IndividualRemedy>();
        Assert.Equivalent(streamedIndividualRemedies, expectedIndividualRemedies);
    }

    [Theory]
    [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
    public async Task Stream_WhenFilteredByName_ShouldReturnFilteredIndividualRemedies(StringFilter filter,
        string name,
        bool expected)
    {
        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedy = new IndividualRemedy
        {
            Id = 123,
            Name = name
        };
        await client.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });
        var streamedIndividualRemedies =
            await ReadIndividualRemedyStreamAsync(client, new IndividualRemedyFilter { Name = filter });
        var expectedIndividualRemedies = expected ? [individualRemedy] : new List<IndividualRemedy>();
        Assert.Equivalent(streamedIndividualRemedies, expectedIndividualRemedies);
    }

    [Fact]
    public async Task Stream_WhenIndividualRemedyCountExceedsBatchSize_ShouldStreamAllIndividualRemedies()
    {
        AddAdditionalHostConfiguration(
            ReplaceOption(
                new StreamingSettings { BatchSize = 3 }
            )
        );

        var client = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);

        var individualRemedies = Enumerable
            .Range(0, 10)
            .Select(iterator
                => new IndividualRemedy { Id = iterator }
            );

        foreach (var individualRemedy in individualRemedies)
            await client.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });

        var streamedIndividualRemedies = await ReadIndividualRemedyStreamAsync(client);
        Assert.Equivalent(streamedIndividualRemedies, individualRemedies);
    }

    private static async Task<IReadOnlyCollection<IndividualRemedy>> ReadIndividualRemedyStreamAsync(
        IndividualRemedyService.IndividualRemedyServiceClient client,
        IndividualRemedyFilter? filter = null)
    {
        var request = new StreamIndividualRemediesRequest
        {
            Filter = filter
        };
        using var streamCall = client.Stream(request);
        var streamedIndividualRemedies = new List<IndividualRemedy>();
        await foreach (var streamedIndividualRemedy in streamCall.ResponseStream.ReadAllAsync())
        {
            streamedIndividualRemedies.Add(streamedIndividualRemedy);
        }

        return streamedIndividualRemedies;
    }
}