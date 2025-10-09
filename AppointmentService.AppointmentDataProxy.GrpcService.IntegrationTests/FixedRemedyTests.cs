using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class FixedRemedyTests(
    GrpcServiceTestFixture<Program> serviceTestFixture,
    KeycloakTestFixture keycloakTestFixture,
    ITestOutputHelper outputHelper) : IntegrationTestBase(serviceTestFixture, keycloakTestFixture, outputHelper)
{
    protected override bool CreateAuthenticatedChannel => false;
    protected override string[] TokenScopes => [];

    [Fact]
    public async Task Stream_WhenFixedRemedyCreated_ShouldReturnFixedRemedy()
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = "456",
            Name = "John Doe",
        };

        await client.CreateAsync(new CreateFixedRemedyRequest
        {
            FixedRemedy = fixedRemedy
        });
        var streamedFixedRemedies = await ReadFixedRemedyStreamAsync(client);
        Assert.Single(streamedFixedRemedies, fixedRemedy);
    }

    [Fact]
    public async Task Stream_WhenFixedRemedyCreatedAndUpdated_ShouldContainUpdatedFixedRemedy()
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);
        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = "456",
            Name = "John Doe",
        };
        var updatedFixedRemedy = new FixedRemedy
        {
            DiagnosisCode = fixedRemedy.DiagnosisCode,
            Name = "Sir John Doe"
        };

        await client.CreateAsync(new CreateFixedRemedyRequest
        {
            FixedRemedy = fixedRemedy
        });
        await client.UpdateAsync(new UpdateFixedRemedyRequest { FixedRemedy = updatedFixedRemedy });
        var streamedFixedRemedies = await ReadFixedRemedyStreamAsync(client);
        Assert.Single(streamedFixedRemedies, updatedFixedRemedy);
    }

    [Fact]
    public async Task Get_WhenFixedRemedyCreated_ShouldReturnFixedRemedy()
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = "123",
            Name = "John Doe"
        };

        await client.CreateAsync(new CreateFixedRemedyRequest { FixedRemedy = fixedRemedy });

        var result = await client.GetAsync(new GetFixedRemedyRequest { DiagnosisCode = fixedRemedy.DiagnosisCode });

        Assert.Equivalent(result.FixedRemedy, fixedRemedy);
    }

    [Fact]
    public async Task Update_WhenFixedRemedyDoesNotExists_ShouldThrowNotFound()
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var notExisting = new FixedRemedy
        {
            DiagnosisCode = "123",
            Name = "Nobody",
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.UpdateAsync(new UpdateFixedRemedyRequest { FixedRemedy = notExisting }));

        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task Update_WhenFixedRemedyDeleted_ShouldReturnNotFound()
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = "123",
            Name = "John Doe"
        };

        await client.CreateAsync(new CreateFixedRemedyRequest { FixedRemedy = fixedRemedy });

        await client.DeleteAsync(new DeleteFixedRemedyRequest { DiagnosisCode = fixedRemedy.DiagnosisCode });

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetAsync(new GetFixedRemedyRequest { DiagnosisCode = fixedRemedy.DiagnosisCode }));

        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
    public async Task Stream_WhenFilteredByDiagnosisCode_ShouldReturnFilteredFixedRemedies(StringFilter filter,
        string diagnosisCode,
        bool expected)
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = diagnosisCode,
            Name = "John Doe"
        };
        await client.CreateAsync(new CreateFixedRemedyRequest { FixedRemedy = fixedRemedy });
        var streamedFixedRemedies =
            await ReadFixedRemedyStreamAsync(client, new FixedRemedyFilter { DiagnosisCode = filter });
        var expectedFixedRemedies = expected ? [fixedRemedy] : new List<FixedRemedy>();
        Assert.Equivalent(streamedFixedRemedies, expectedFixedRemedies);
    }

    [Theory]
    [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
    public async Task Stream_WhenFilteredByName_ShouldReturnFilteredFixedRemedies(StringFilter filter,
        string name,
        bool expected)
    {
        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedy = new FixedRemedy
        {
            DiagnosisCode = "123",
            Name = name
        };
        await client.CreateAsync(new CreateFixedRemedyRequest { FixedRemedy = fixedRemedy });
        var streamedFixedRemedies = await ReadFixedRemedyStreamAsync(client, new FixedRemedyFilter { Name = filter });
        var expectedFixedRemedies = expected ? [fixedRemedy] : new List<FixedRemedy>();
        Assert.Equivalent(streamedFixedRemedies, expectedFixedRemedies);
    }

    [Fact]
    public async Task Stream_WhenFixedRemedyCountExceedsBatchSize_ShouldStreamAllFixedRemedies()
    {
        AddAdditionalHostConfiguration(
            ReplaceOption(
                new StreamingSettings { BatchSize = 3 }
            )
        );

        var client = new FixedRemedyService.FixedRemedyServiceClient(Channel);

        var fixedRemedies = Enumerable
            .Range(0, 10)
            .Select(iterator
                => new FixedRemedy { DiagnosisCode = iterator.ToString() }
            );

        foreach (var fixedRemedy in fixedRemedies)
            await client.CreateAsync(new CreateFixedRemedyRequest { FixedRemedy = fixedRemedy });

        var streamedFixedRemedies = await ReadFixedRemedyStreamAsync(client);
        Assert.Equivalent(streamedFixedRemedies, fixedRemedies);
    }

    private static async Task<IReadOnlyCollection<FixedRemedy>> ReadFixedRemedyStreamAsync(
        FixedRemedyService.FixedRemedyServiceClient client,
        FixedRemedyFilter? filter = null)
    {
        var request = new StreamFixedRemediesRequest
        {
            Filter = filter
        };
        using var streamCall = client.Stream(request);
        var streamedFixedRemedies = new List<FixedRemedy>();
        await foreach (var streamedFixedRemedy in streamCall.ResponseStream.ReadAllAsync())
        {
            streamedFixedRemedies.Add(streamedFixedRemedy);
        }

        return streamedFixedRemedies;
    }
}