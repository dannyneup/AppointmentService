using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class PatientTests(GrpcServiceTestFixture<Program> serviceTestFixture, ITestOutputHelper outputHelper) : IntegrationTestBase(serviceTestFixture, outputHelper)
    {
        [Fact]
        public async Task Stream_WhenPatientCreated_ShouldReturnPatient()
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                Age = 123,
                InsuranceNumber = "456",
                Name = "John Doe",
            };

            await client.CreateAsync(new CreatePatientRequest
            {
                Patient = patient
            });
            var streamedPatients = await ReadPatientStreamAsync(client);
            Assert.Single(streamedPatients, patient);
        }

        [Fact]
        public async Task Stream_WhenPatientCreatedAndUpdated_ShouldContainUpdatedPatient()
        {
            var client = new PatientService.PatientServiceClient(Channel);
            var patient = new Patient
            {
                Age = 123,
                InsuranceNumber = "456",
                Name = "John Doe",
            };
            var updatedPatient = new Patient
            {
                Age = 456,
                InsuranceNumber = patient.InsuranceNumber,
                Name = "Sir John Doe"
            };

            await client.CreateAsync(new CreatePatientRequest
            {
                Patient = patient
            });
            await client.UpdateAsync(new UpdatePatientRequest { Patient = updatedPatient });
            var streamedPatients = await ReadPatientStreamAsync(client);
            Assert.Single(streamedPatients, updatedPatient);
        }

        [Fact]
        public async Task Get_WhenPatientCreated_ShouldReturnPatient()
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                Age = 324,
                InsuranceNumber = "123",
                Name = "John Doe"
            };

            await client.CreateAsync(new CreatePatientRequest { Patient = patient });

            var result = await client.GetAsync(new GetPatientRequest { InsuranceNumber = patient.InsuranceNumber });

            Assert.Equivalent(result.Patient, patient);
        }

        [Fact]
        public async Task Update_WhenPatientDoesNotExists_ShouldThrowNotFound()
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var notExisting = new Patient
            {
                InsuranceNumber = Guid.NewGuid().ToString(),
                Name = "Nobody",
                Age = 99
            };

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.UpdateAsync(new UpdatePatientRequest { Patient = notExisting }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task Update_WhenPatientDeleted_ShouldReturnNotFound()
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                InsuranceNumber = "123",
                Age = 324,
                Name = "John Doe"
            };

            await client.CreateAsync(new CreatePatientRequest { Patient = patient });

            await client.DeleteAsync(new DeletePatientRequest { InsuranceNumber = patient.InsuranceNumber });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.GetAsync(new GetPatientRequest { InsuranceNumber = patient.InsuranceNumber }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByInsuranceNumber_ShouldReturnFilteredPatients(StringFilter filter, string insuranceNumber, bool expected)
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                InsuranceNumber = insuranceNumber,
                Name = "John Doe"
            };
            await client.CreateAsync(new CreatePatientRequest { Patient = patient });
            var streamedPatients = await ReadPatientStreamAsync(client, new PatientFilter{InsuranceNumber = filter});
            var expectedPatients = expected ? [patient] : new List<Patient>();
            Assert.Equivalent(streamedPatients, expectedPatients);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByName_ShouldReturnFilteredPatients(StringFilter filter, string name, bool expected)
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                InsuranceNumber = "0123",
                Name = name
            };
            await client.CreateAsync(new CreatePatientRequest { Patient = patient });
            var streamedPatients = await ReadPatientStreamAsync(client, new PatientFilter{Name = filter});
            var expectedPatients = expected ? [patient] : new List<Patient>();
            Assert.Equivalent(streamedPatients, expectedPatients);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByAge_ShouldReturnFilteredPatients(Int32Filter filter, int age, bool expected)
        {
            var client = new PatientService.PatientServiceClient(Channel);

            var patient = new Patient
            {
                InsuranceNumber = "0123",
                Age = age
            };
            await client.CreateAsync(new CreatePatientRequest { Patient = patient });
            var streamedPatients = await ReadPatientStreamAsync(client, new PatientFilter{Age = filter});
            var expectedPatients = expected ? [patient] : new List<Patient>();
            Assert.Equivalent(streamedPatients, expectedPatients);
        }

        [Fact]
        public async Task Stream_WhenPatientCountExceedsBatchSize_ShouldStreamAllPatients()
        {
            AddAdditionalHostConfiguration(
                ReplaceOption(
                    new StreamingSettings { BatchSize = 5 }
                )
            );

            var client = new PatientService.PatientServiceClient(Channel);

            var patients = Enumerable
                .Range(0, 10)
                .Select(iterator
                    => new Patient
                        { InsuranceNumber = iterator.ToString() }
                );

            foreach (var patient in patients)
                await client.CreateAsync(new CreatePatientRequest { Patient = patient });

            var streamedPatients = await ReadPatientStreamAsync(client);
            Assert.Equivalent(streamedPatients, patients);
        }

        private static async Task<IReadOnlyCollection<Patient>> ReadPatientStreamAsync(PatientService.PatientServiceClient client, PatientFilter? filter = null)
        {
            var request = new StreamPatientsRequest
            {
                Filter = filter
            };
            using var streamCall = client.Stream(request);
            var streamedPatients = new List<Patient>();
            await foreach (var streamedPatient in streamCall.ResponseStream.ReadAllAsync())
            {
                streamedPatients.Add(streamedPatient);
            }
            return streamedPatients;
        }
    }