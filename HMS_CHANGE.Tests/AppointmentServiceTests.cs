//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using Moq;
//using Microsoft.EntityFrameworkCore;
//using AutoMapper;
//using DataAL.Models; // Namespace containing your entities
//using BusinessLL;
//using Microsoft.EntityFrameworkCore.Diagnostics;    // Namespace containing AppointmentService and AppointmentValidationService

//namespace HMS_CHANGE.Tests.Patient.AppointmentServiceTestsNamespace // Renamed to avoid conflict
//{
//    public class AppointmentServiceTests
//    {
//        private readonly IMapper _mapper;

//        public AppointmentServiceTests()
//        {
//            // Configure AutoMapper with necessary profiles
//            var config = new MapperConfiguration(cfg =>
//            {
//                cfg.AddProfile<AutoMapperProfile>(); // Ensure your AutoMapperProfile is correctly referenced
//            });
//            _mapper = config.CreateMapper();
//        }

//        [Fact]
//        public async Task CreateAppointment_ShouldNotModifyUserEntity()
//        {
//            // Arrange
//            var options = new DbContextOptionsBuilder<HmsAContext>()
//                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
//                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Ignore transaction warnings
//                .EnableSensitiveDataLogging() // Enable sensitive data logging
//                .Options;

//            Guid patientUserId = Guid.NewGuid();
//            Guid doctorUserId = Guid.NewGuid();
//            Guid departmentId = Guid.NewGuid();
//            Guid doctorDetailId = Guid.NewGuid();
//            Guid availabilityId = Guid.NewGuid();

//            // Set appointmentDateUtc within valid UTC hours
//            DateTime appointmentDateUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(10); // Tomorrow at 10 AM UTC

//            // Seed the in-memory database with test data
//            using (var seedContext = new HmsAContext(options))
//            {
//                // Add Roles (Ensure consistency with UserService.cs)
//                var roles = new List<Role>
//                        {
//                            new Role { RoleId = new Guid("4697688e-b27b-411e-8ff2-e6a0126c68d6"), RoleName = "Admin" },
//                            new Role { RoleId = new Guid("9d8eb0a2-2954-49c4-a933-612d766d3fa5"), RoleName = "Doctor" },
//                            new Role { RoleId = new Guid("803dcef7-b173-4799-9ac5-48da9be43f2e"), RoleName = "Patient" }
//                        };
//                seedContext.Roles.AddRange(roles);

//                // Add Users
//                var patient = new User
//                {
//                    UserId = patientUserId,
//                    Name = "Test Patient",
//                    Email = "patient@example.com",
//                    PhoneNumber = "1234567890",
//                    PasswordHash = "hashedpassword",
//                    RoleId = new Guid("803dcef7-b173-4799-9ac5-48da9be43f2e"), // Patient Role
//                    Gender = "Female",
//                    IsActive = true,
//                    IsDeleted = false,
//                    DateOfBirth = new DateOnly(1990, 1, 1)
//                };

//                var doctor = new User
//                {
//                    UserId = doctorUserId,
//                    Name = "Test Doctor",
//                    Email = "doctor@example.com",
//                    PhoneNumber = "0987654321",
//                    PasswordHash = "hashedpassword",
//                    RoleId = new Guid("9d8eb0a2-2954-49c4-a933-612d766d3fa5"), // Doctor Role
//                    Gender = "Male",
//                    IsActive = true,
//                    IsDeleted = false,
//                    DateOfBirth = new DateOnly(1980, 5, 20)
//                };

//                seedContext.Users.AddRange(patient, doctor);

//                // Add Department
//                var department = new Department
//                {
//                    DepartmentId = departmentId,
//                    DepartmentName = "Cardiology",
//                    DepartmentCode = "CARD"
//                };
//                seedContext.Departments.Add(department);

//                // Add DoctorDetail
//                var doctorDetail = new DoctorDetail
//                {
//                    DoctorId = doctorDetailId,
//                    UserId = doctorUserId,
//                    Specialization = "Cardiologist",
//                    Phone = "0987654321",
//                    DepartmentId = departmentId,
//                    Qualification = "MD",
//                    ExperienceYears = 10,
//                    Description = "Experienced cardiologist."
//                };
//                seedContext.DoctorDetails.Add(doctorDetail);

//                // Add DoctorAvailability covering the appointment time
//                var availability = new DoctorAvailability
//                {
//                    AvailabilityId = availabilityId,
//                    DoctorId = doctorDetailId,
//                    AvailableFrom = appointmentDateUtc.AddHours(-1), // 1 hour before appointment
//                    AvailableTo = appointmentDateUtc.AddHours(1),    // 1 hour after appointment
//                    IsAvailable = true,
//                    IsSlotAvailable = true,
//                    UtcOffset = TimeSpan.Zero // Zero offset as per your decision
//                };
//                seedContext.DoctorAvailabilities.Add(availability);

//                await seedContext.SaveChangesAsync();
//            }

//            // Mock the IDbContextFactory<HmsAContext> to return a new context instance for each operation
//            var mockFactory = new Mock<IDbContextFactory<HmsAContext>>();
//            mockFactory.Setup(cf => cf.CreateDbContext())
//                       .Returns(() => new HmsAContext(options));

//            // Mock IAppointmentValidationService
//            var mockValidationService = new Mock<IAppointmentValidationService>();
//            mockValidationService
//                .Setup(v => v.ValidateAppointmentAsync(It.IsAny<AppointmentDTO>()))
//                .Returns(Task.CompletedTask);

//            var appointmentService = new AppointmentService(
//                mockFactory.Object,
//                mockValidationService.Object,
//                _mapper
//            );

//            // Create AppointmentDTO with UTC time and zero offset
//            var appointmentDto = new AppointmentDTO
//            {
//                AppointmentId = Guid.NewGuid(),
//                DoctorId = doctorDetailId, // Correctly reference DoctorDetailId
//                PatientId = patientUserId,
//                DepartmentId = departmentId,
//                AppointmentDate = appointmentDateUtc, // Use UTC time directly
//                Notes = "Routine Checkup",
//                Status = "Pending",
//                IsActive = true,
//                CreatedAt = DateTime.UtcNow,
//                UtcOffset = TimeSpan.Zero
//            };

//            // Capture the state of the User before appointment creation
//            using (var testContext = new HmsAContext(options))
//            {
//                var userBefore = await testContext.Users.AsNoTracking()
//                                                      .FirstOrDefaultAsync(u => u.UserId == patientUserId);
//                Assert.NotNull(userBefore); // Ensure the user exists

//                // Log the state of the Users table before appointment creation
//                var usersBefore = await testContext.Users.AsNoTracking().ToListAsync();
//                Console.WriteLine("Users before appointment creation:");
//                foreach (var user in usersBefore)
//                {
//                    Console.WriteLine($"UserId: {user.UserId}, Email: {user.Email}");
//                }

//                // Act
//                var result = await appointmentService.CreateAppointmentAsync(
//                    appointmentDto.DoctorId,
//                    appointmentDto.PatientId,
//                    appointmentDto.DepartmentId,
//                    appointmentDto.AppointmentDate, // Pass UTC time directly
//                    appointmentDto.Notes,
//                    appointmentDto.FollowUpAppointmentId
//                );

//                // Assert
//                Assert.NotNull(result);
//                Assert.Equal(appointmentDto.AppointmentId, result.AppointmentId);
//                Assert.Equal("Pending", result.Status);
//                Assert.True(result.IsActive);
//                Assert.Equal(appointmentDto.DoctorId, result.DoctorId);
//                Assert.Equal(appointmentDto.PatientId, result.PatientId);
//                Assert.Equal(appointmentDto.DepartmentId, result.DepartmentId);
//                Assert.Equal(appointmentDto.Notes, result.Notes);
//                Assert.Equal(appointmentDto.CreatedAt, result.CreatedAt);
//                Assert.Equal(appointmentDto.AppointmentDate, result.AppointmentDate);

//                // Verify that the User entity remains unchanged
//                var userAfter = await testContext.Users.AsNoTracking()
//                                                     .FirstOrDefaultAsync(u => u.UserId == patientUserId);
//                Assert.NotNull(userAfter);

//                // Compare each relevant property to ensure no changes
//                Assert.Equal(userBefore.Name, userAfter.Name);
//                Assert.Equal(userBefore.Email, userAfter.Email);
//                Assert.Equal(userBefore.PhoneNumber, userAfter.PhoneNumber);
//                Assert.Equal(userBefore.PasswordHash, userAfter.PasswordHash);
//                Assert.Equal(userBefore.RoleId, userAfter.RoleId);
//                Assert.Equal(userBefore.Gender, userAfter.Gender);
//                Assert.Equal(userBefore.IsActive, userAfter.IsActive);
//                Assert.Equal(userBefore.IsDeleted, userAfter.IsDeleted);
//                Assert.Equal(userBefore.DateOfBirth, userAfter.DateOfBirth);

//                // Additionally, verify that no extra Users were added
//                var userCount = await testContext.Users.CountAsync();
//                Assert.Equal(2, userCount); // Only patient and doctor should exist

//                // Log the state of the Users table after appointment creation
//                var usersAfter = await testContext.Users.AsNoTracking().ToListAsync();
//                Console.WriteLine("Users after appointment creation:");
//                foreach (var user in usersAfter)
//                {
//                    Console.WriteLine($"UserId: {user.UserId}, Email: {user.Email}");
//                }

//                // Verify that ValidateAppointmentAsync was called once
//                mockValidationService.Verify(v => v.ValidateAppointmentAsync(It.IsAny<AppointmentDTO>()), Times.Once);
//            }
//        }
//    }
//}
