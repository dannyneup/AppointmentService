using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class TherapistTests(GrpcServiceTestFixture<Program> serviceTestFixture, KeycloakTestFixture keycloakTestFixture, ITestOutputHelper outputHelper) : IntegrationTestBase(serviceTestFixture, keycloakTestFixture, outputHelper)
    {
        [Fact]
        public async Task Stream_WhenTherapistCreated_ShouldReturnTherapist()
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapist = new Therapist
            {
                Id = 456,
                Name = "John Doe",
            };

            await client.CreateAsync(new CreateTherapistRequest
            {
                Therapist = therapist
            });
            var streamedTherapists = await ReadTherapistStreamAsync(client);
            Assert.Single(streamedTherapists, therapist);
        }

        [Fact]
        public async Task Stream_WhenTherapistCreatedAndUpdated_ShouldContainUpdatedTherapist()
        {
            var client = new TherapistService.TherapistServiceClient(Channel);
            var therapist = new Therapist
            {
                Id = 456,
                Name = "John Doe",
            };
            var updatedTherapist = new Therapist
            {
                Id = therapist.Id,
                Name = "Sir John Doe"
            };

            await client.CreateAsync(new CreateTherapistRequest
            {
                Therapist = therapist
            });
            await client.UpdateAsync(new UpdateTherapistRequest { Therapist = updatedTherapist });
            var streamedTherapists = await ReadTherapistStreamAsync(client);
            Assert.Single(streamedTherapists, updatedTherapist);
        }

        [Fact]
        public async Task Get_WhenTherapistCreated_ShouldReturnTherapist()
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapist = new Therapist
            {
                Id = 123,
                Name = "John Doe"
            };

            await client.CreateAsync(new CreateTherapistRequest { Therapist = therapist });

            var result = await client.GetAsync(new GetTherapistRequest { Id = therapist.Id });

            Assert.Equivalent(result.Therapist, therapist);
        }

        [Fact]
        public async Task Update_WhenTherapistDoesNotExists_ShouldThrowNotFound()
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var notExisting = new Therapist
            {
                Id = 123,
                Name = "Nobody",
            };

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.UpdateAsync(new UpdateTherapistRequest { Therapist = notExisting }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task Update_WhenTherapistDeleted_ShouldReturnNotFound()
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapist = new Therapist
            {
                Id = 123,
                Name = "John Doe"
            };

            await client.CreateAsync(new CreateTherapistRequest { Therapist = therapist });

            await client.DeleteAsync(new DeleteTherapistRequest { Id = therapist.Id });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.GetAsync(new GetTherapistRequest { Id = therapist.Id }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredById_ShouldReturnFilteredTherapists(Int32Filter filter, int id, bool expected)
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapist = new Therapist
            {
                Id = id,
                Name = "John Doe"
            };
            await client.CreateAsync(new CreateTherapistRequest { Therapist = therapist });
            var streamedTherapists = await ReadTherapistStreamAsync(client, new TherapistFilter{Id = filter});
            var expectedTherapists = expected ? [therapist] : new List<Therapist>();
            Assert.Equivalent(streamedTherapists, expectedTherapists);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByName_ShouldReturnFilteredTherapists(StringFilter filter, string name, bool expected)
        {
            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapist = new Therapist
            {
                Id = 123,
                Name = name
            };
            await client.CreateAsync(new CreateTherapistRequest { Therapist = therapist });
            var streamedTherapists = await ReadTherapistStreamAsync(client, new TherapistFilter{Name = filter});
            var expectedTherapists = expected ? [therapist] : new List<Therapist>();
            Assert.Equivalent(streamedTherapists, expectedTherapists);
        }

        [Fact]
        public async Task Stream_WhenTherapistCountExceedsBatchSize_ShouldStreamAllTherapists()
        {
            AddAdditionalHostConfiguration(
                ReplaceOption(
                    new StreamingSettings { BatchSize = 3 }
                )
            );

            var client = new TherapistService.TherapistServiceClient(Channel);

            var therapists = Enumerable
                .Range(0, 10)
                .Select(iterator
                    => new Therapist { Id = iterator }
                );

            foreach (var therapist in therapists)
                await client.CreateAsync(new CreateTherapistRequest { Therapist = therapist });

            var streamedTherapists = await ReadTherapistStreamAsync(client);
            Assert.Equivalent(streamedTherapists, therapists);
        }

        private static async Task<IReadOnlyCollection<Therapist>> ReadTherapistStreamAsync(TherapistService.TherapistServiceClient client, TherapistFilter? filter = null)
        {
            var request = new StreamTherapistsRequest
            {
                Filter = filter
            };
            using var streamCall = client.Stream(request);
            var streamedTherapists = new List<Therapist>();
            await foreach (var streamedTherapist in streamCall.ResponseStream.ReadAllAsync())
            {
                streamedTherapists.Add(streamedTherapist);
            }
            return streamedTherapists;
        }
    }