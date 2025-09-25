using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class PracticeTests(GrpcServiceTestFixture<Program> serviceTestFixture, ITestOutputHelper outputHelper) : IntegrationTestBase(serviceTestFixture, outputHelper)
    {
        [Fact]
        public async Task Stream_WhenPracticeCreated_ShouldReturnPractice()
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var practice = new Practice
            {
                InstitutionCode = "456",
                Name = "John Does Physiotherapy",
            };

            await client.CreateAsync(new CreatePracticeRequest
            {
                Practice = practice
            });
            var streamedPractices = await ReadPracticeStreamAsync(client);
            Assert.Single(streamedPractices, practice);
        }

        [Fact]
        public async Task Stream_WhenPracticeCreatedAndUpdated_ShouldContainUpdatedPractice()
        {
            var client = new PracticeService.PracticeServiceClient(Channel);
            var practice = new Practice
            {
                InstitutionCode = "456",
                Name = "John Does Physiotherapy",
            };
            var updatedPractice = new Practice
            {
                InstitutionCode = practice.InstitutionCode,
                Name = "Sir John Does Physiotherapy",
            };

            await client.CreateAsync(new CreatePracticeRequest
            {
                Practice = practice
            });
            await client.UpdateAsync(new UpdatePracticeRequest { Practice = updatedPractice });
            var streamedPractices = await ReadPracticeStreamAsync(client);
            Assert.Single(streamedPractices, updatedPractice);
        }

        [Fact]
        public async Task Get_WhenPracticeCreated_ShouldReturnPractice()
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var practice = new Practice
            {
                InstitutionCode = "123",
                Name = "John Does Physiotherapy"
            };

            await client.CreateAsync(new CreatePracticeRequest { Practice = practice });

            var result = await client.GetAsync(new GetPracticeRequest { InstitutionCode = practice.InstitutionCode });

            Assert.Equivalent(result.Practice, practice);
        }

        [Fact]
        public async Task Update_WhenPracticeDoesNotExists_ShouldThrowNotFound()
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var notExisting = new Practice
            {
                InstitutionCode = "123",
                Name = "Nobody",
            };

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.UpdateAsync(new UpdatePracticeRequest { Practice = notExisting }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task Update_WhenPracticeDeleted_ShouldReturnNotFound()
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var practice = new Practice
            {
                InstitutionCode = "123",
                Name = "John Does Physiotherapy"
            };

            await client.CreateAsync(new CreatePracticeRequest { Practice = practice });

            await client.DeleteAsync(new DeletePracticeRequest { InstitutionCode = practice.InstitutionCode });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.GetAsync(new GetPracticeRequest { InstitutionCode = practice.InstitutionCode }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByInstitutionCode_ShouldReturnFilteredPractices(StringFilter filter, string id, bool expected)
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var practice = new Practice
            {
                InstitutionCode = id,
                Name = "John Does Physiotherapy"
            };
            await client.CreateAsync(new CreatePracticeRequest { Practice = practice });
            var streamedPractices = await ReadPracticeStreamAsync(client, new PracticeFilter{ InstitutionCode = filter });
            var expectedPractices = expected ? [practice] : new List<Practice>();
            Assert.Equivalent(streamedPractices, expectedPractices);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByName_ShouldReturnFilteredPractices(StringFilter filter, string name, bool expected)
        {
            var client = new PracticeService.PracticeServiceClient(Channel);

            var practice = new Practice
            {
                InstitutionCode = "123",
                Name = name
            };
            await client.CreateAsync(new CreatePracticeRequest { Practice = practice });
            var streamedPractices = await ReadPracticeStreamAsync(client, new PracticeFilter{ Name = filter });
            var expectedPractices = expected ? [practice] : new List<Practice>();
            Assert.Equivalent(streamedPractices, expectedPractices);
        }

        [Fact]
        public async Task Stream_WhenPracticeCountExceedsBatchSize_ShouldStreamAllPractices()
        {
            AddAdditionalHostConfiguration(
                ReplaceOption(
                    new StreamingSettings { BatchSize = 3 }
                )
            );

            var client = new PracticeService.PracticeServiceClient(Channel);

            var practices = Enumerable
                .Range(0, 10)
                .Select(iterator
                    => new Practice { InstitutionCode = iterator.ToString() }
                );

            foreach (var practice in practices)
                await client.CreateAsync(new CreatePracticeRequest { Practice = practice });

            var streamedPractices = await ReadPracticeStreamAsync(client);
            Assert.Equivalent(streamedPractices, practices);
        }

        private static async Task<IReadOnlyCollection<Practice>> ReadPracticeStreamAsync(PracticeService.PracticeServiceClient client, PracticeFilter? filter = null)
        {
            var request = new StreamPracticesRequest
            {
                Filter = filter
            };
            using var streamCall = client.Stream(request);
            var streamedPractices = new List<Practice>();
            await foreach (var streamedPractice in streamCall.ResponseStream.ReadAllAsync())
            {
                streamedPractices.Add(streamedPractice);
            }
            return streamedPractices;
        }
    }