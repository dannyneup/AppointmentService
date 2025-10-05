using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers;
using AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;
using AppointmentService.AppointmentDataProxy.GrpcService.Protos;
using AppointmentService.AppointmentDataProxy.GrpcService.Shared.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Xunit.Abstractions;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests;

public class AppointmentTests : IntegrationTestBase
    {
        private readonly Protos.AppointmentService.AppointmentServiceClient _appointmentServiceClient;
        private readonly TherapistService.TherapistServiceClient _therapistServiceClient;
        private readonly IndividualRemedyService.IndividualRemedyServiceClient _individualRemedyServiceClient;
        private readonly FixedRemedyService.FixedRemedyServiceClient _fixedRemedyServiceClient;

        private readonly Therapist _testTherapist = new()
        {
            Id = 123,
            Name = "John Doe"
        };

        private readonly IndividualRemedy _testIndividualRemedy = new()
        {
            Id = 123,
            Name = "some individual Remedy"
        };

        private readonly FixedRemedy _testFixedRemedy = new()
        {
            DiagnosisCode = "012ABC",
            Name = "some fixed Remedy"
        };

        public AppointmentTests(GrpcServiceTestFixture<Program> serviceTestFixture, ITestOutputHelper outputHelper) : base(serviceTestFixture, outputHelper)
        {
            _appointmentServiceClient = new Protos.AppointmentService.AppointmentServiceClient(Channel);
            _therapistServiceClient = new TherapistService.TherapistServiceClient(Channel);
            _individualRemedyServiceClient = new IndividualRemedyService.IndividualRemedyServiceClient(Channel);
            _fixedRemedyServiceClient = new FixedRemedyService.FixedRemedyServiceClient(Channel);
        }

        [Fact]
        public async Task Stream_WhenReferenceEntitiesAndAppointmentCreated_ShouldReturnAppointment()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest
            {
                Appointment = appointment
            });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient);
            Assert.Single(streamedAppointments, appointment);
        }

        [Fact]
        public async Task Stream_WhenAppointmentCreatedAndUpdated_ShouldContainUpdatedAppointment()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            var updatedAppointment = new Appointment
            {
                Id = appointment.Id,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 2, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 3, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "654",
                PracticeInstitutionCode = "987",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest
            {
                Appointment = appointment
            });
            await _appointmentServiceClient.UpdateAsync(new UpdateAppointmentRequest { Appointment = updatedAppointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient);
            Assert.Single(streamedAppointments, updatedAppointment);
        }

        [Fact]
        public async Task Stream_WhenAppointmentCreatedAndUpdatedWithRemedyTypeChange_ShouldContainUpdatedAppointment()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            var updatedAppointment = new Appointment
            {
                Id = appointment.Id,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 2, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 3, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "654",
                PracticeInstitutionCode = "987",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = _testFixedRemedy.DiagnosisCode,
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest
            {
                Appointment = appointment
            });
            await CreateTestFixedRemedy();
            await _appointmentServiceClient.UpdateAsync(new UpdateAppointmentRequest { Appointment = updatedAppointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient);
            Assert.Single(streamedAppointments, updatedAppointment);
        }

        [Fact]
        public async Task Get_WhenReferenceEntitiesAndAppointmentCreated_ShouldReturnAppointment()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });

            var result = await _appointmentServiceClient.GetAsync(new GetAppointmentRequest { Id = appointment.Id });

            Assert.Equivalent(result.Appointment, appointment);
        }

        [Fact]
        public async Task Create_WhenExternalDbReferenceEntitiesDoNotExist_ShouldThrowException()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = "non existing diagnosis code"
            };

            await CreateTestTherapist();
            await CreateTestFixedRemedy();

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest{ Appointment = appointment }));

            Assert.Equivalent(StatusCode.FailedPrecondition, exception.StatusCode);
        }

        [Fact]
        public async Task Create_WhenInternalReferenceEntitiesDoNotExist_ShouldThrowException()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id + 100,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest{ Appointment = appointment }));

            Assert.Equivalent(StatusCode.FailedPrecondition, exception.StatusCode);
        }

        [Fact]
        public async Task Update_WhenExternalDbReferenceEntitiesDoNotExist_ShouldThrowException()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = _testFixedRemedy.DiagnosisCode
            };

            var updatedAppointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = "non existing diagnosis code"
            };

            await CreateTestTherapist();
            await CreateTestFixedRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest{ Appointment = appointment });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await _appointmentServiceClient.UpdateAsync(new UpdateAppointmentRequest{ Appointment = updatedAppointment }));

            Assert.Equivalent(StatusCode.FailedPrecondition, exception.StatusCode);
        }

        [Fact]
        public async Task Update_WhenInternalDbReferenceEntitiesDoNotExist_ShouldThrowException()
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = _testFixedRemedy.DiagnosisCode
            };

            var updatedAppointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id + 100,
                FixedRemedyDiagnosisCode = _testFixedRemedy.DiagnosisCode
            };

            await CreateTestTherapist();
            await CreateTestFixedRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest{ Appointment = appointment });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await _appointmentServiceClient.UpdateAsync(new UpdateAppointmentRequest{ Appointment = updatedAppointment }));

            Assert.Equivalent(StatusCode.FailedPrecondition, exception.StatusCode);
        }

        [Fact]
        public async Task Update_WhenAppointmentDoesNotExists_ShouldThrowNotFound()
        {
            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var notExisting = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.UpdateAsync(new UpdateAppointmentRequest { Appointment = notExisting }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task Update_WhenAppointmentDeleted_ShouldReturnNotFound()
        {
            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await client.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });

            await client.DeleteAsync(new DeleteAppointmentRequest { Id = appointment.Id });

            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
                await client.GetAsync(new GetAppointmentRequest { Id = appointment.Id }));

            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredById_ShouldReturnFilteredAppointments(Int32Filter filter, int id, bool expected)
        {
            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var appointment = new Appointment
            {
                Id = id,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await client.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(client, new AppointmentFilter{Id = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.TimestampCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByStart_ShouldReturnFilteredAppointments(GoogleTimestampFilter filter, Timestamp start, bool expected)
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = start,
                End = Timestamp.FromDateTimeOffset(start.ToDateTimeOffset().AddHours(1)),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient, new AppointmentFilter{Start = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.TimestampCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByEnd_ShouldReturnFilteredAppointments(GoogleTimestampFilter filter, Timestamp end, bool expected)
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(end.ToDateTimeOffset().AddHours(-1)),
                End = end,
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient, new AppointmentFilter{End = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByPatientInsuranceNumber_ShouldReturnFilteredAppointments(StringFilter filter, string insuranceNumber, bool expected)
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = insuranceNumber,
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient, new AppointmentFilter{PatientInsuranceNumber = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByTherapistId_ShouldReturnFilteredAppointments(Int32Filter filter, int therapistId, bool expected)
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = therapistId,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            var therapist = new Therapist
            {
                Id = therapistId,
                Name = "John Doe"
            };
            await _therapistServiceClient.CreateAsync(new CreateTherapistRequest{Therapist = therapist});
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient, new AppointmentFilter{TherapistId = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByPracticeInstitutionCode_ShouldReturnFilteredAppointments(StringFilter filter, string practiceInstitutionCode, bool expected)
        {
            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = practiceInstitutionCode,
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = _testIndividualRemedy.Id
            };
            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            await _appointmentServiceClient.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(_appointmentServiceClient, new AppointmentFilter{PracticeInstitutionCode = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.StringCases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByFixedRemedyDiagnosisCode_ShouldReturnFilteredAppointments(StringFilter filter, string fixedRemedyDiagnosisCode, bool expected)
        {
            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                FixedRemedyDiagnosisCode = fixedRemedyDiagnosisCode
            };
            var fixedRemedy = new FixedRemedy
            {
                DiagnosisCode = fixedRemedyDiagnosisCode,
                Name = "some fixed remedy"
            };
            await CreateTestTherapist();
            await _fixedRemedyServiceClient.CreateAsync(new  CreateFixedRemedyRequest{FixedRemedy = fixedRemedy});
            await client.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(client, new AppointmentFilter{FixedRemedyDiagnosisCode = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Theory]
        [MemberData(nameof(SharedFilterCases.Int32Cases), MemberType = typeof(SharedFilterCases))]
        public async Task Stream_WhenFilteredByIndividualRemedyId_ShouldReturnFilteredAppointments(Int32Filter filter, int individualRemedyId, bool expected)
        {
            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var appointment = new Appointment
            {
                Id = 123,
                Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                PatientInsuranceNumber = "456",
                PracticeInstitutionCode = "789",
                TherapistId = _testTherapist.Id,
                IndividualRemedyId = individualRemedyId
            };
            var individualRemedy = new IndividualRemedy
            {
                Id = individualRemedyId,
                Name = "some individual Remedy"
            };
            await CreateTestTherapist();
            await _individualRemedyServiceClient.CreateAsync(new CreateIndividualRemedyRequest { IndividualRemedy = individualRemedy });
            await client.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });
            var streamedAppointments = await ReadAppointmentStreamAsync(client, new AppointmentFilter{IndividualRemedyId = filter});
            var expectedAppointments = expected ? [appointment] : new List<Appointment>();
            Assert.Equivalent(streamedAppointments, expectedAppointments);
        }

        [Fact]
        public async Task Stream_WhenAppointmentCountExceedsBatchSize_ShouldStreamAllAppointments()
        {
            AddAdditionalHostConfiguration(
                ReplaceOption(
                    new StreamingSettings { BatchSize = 3 }
                )
            );

            var client = new Protos.AppointmentService.AppointmentServiceClient(Channel);

            var appointments = Enumerable
                .Range(0, 10)
                .Select(iterator
                    => new Appointment
                    {
                        Id = iterator,
                        Start = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-3, 0, 0))),
                        End = Timestamp.FromDateTimeOffset(new DateTimeOffset(2025, 1, 1, 23, 49, 0, new TimeSpan(-2, 0, 0))),
                        PatientInsuranceNumber = "456",
                        PracticeInstitutionCode = "789",
                        TherapistId = _testTherapist.Id,
                        IndividualRemedyId = _testIndividualRemedy.Id
                    }
                );

            await CreateTestTherapist();
            await CreateTestIndividualRemedy();
            foreach (var appointment in appointments)
                await client.CreateAsync(new CreateAppointmentRequest { Appointment = appointment });

            var streamedAppointments = await ReadAppointmentStreamAsync(client);
            Assert.Equivalent(streamedAppointments, appointments);
        }

        private static async Task<IReadOnlyCollection<Appointment>> ReadAppointmentStreamAsync(Protos.AppointmentService.AppointmentServiceClient client, AppointmentFilter? filter = null)
        {
            var request = new StreamAppointmentsRequest
            {
                Filter = filter
            };
            using var streamCall = client.Stream(request);
            var streamedAppointments = new List<Appointment>();
            await foreach (var streamedAppointment in streamCall.ResponseStream.ReadAllAsync())
            {
                streamedAppointments.Add(streamedAppointment);
            }
            return streamedAppointments;
        }

        private Task CreateTestTherapist()
            => _therapistServiceClient.CreateAsync(new CreateTherapistRequest
            {
                Therapist = _testTherapist
            }).ResponseAsync;

        private Task CreateTestIndividualRemedy()
            => _individualRemedyServiceClient.CreateAsync(new CreateIndividualRemedyRequest
            {
                IndividualRemedy = _testIndividualRemedy
            }).ResponseAsync;

        private Task CreateTestFixedRemedy()
            => _fixedRemedyServiceClient.CreateAsync(new CreateFixedRemedyRequest
            {
                FixedRemedy = _testFixedRemedy
            }).ResponseAsync;
    }