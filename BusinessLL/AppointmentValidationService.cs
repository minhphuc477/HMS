using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class AppointmentValidationService : IAppointmentValidationService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;

        public AppointmentValidationService(IDbContextFactory<HmsAContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task ValidateAppointmentAsync(AppointmentDTO appointmentDto)
        {
            try
            {
                var tasks = new[]
                {
                            CheckDoctorExistsAsync(appointmentDto.DoctorId),
                            CheckPatientExistsAsync(appointmentDto.PatientId),
                            CheckDepartmentExistsAsync(appointmentDto.DepartmentId),
                            CheckOverlappingAppointmentsAsync(appointmentDto),
                            CheckDoctorAvailabilityAsync(appointmentDto),
                            CheckAppointmentTimeAsync(appointmentDto.AppointmentDate, appointmentDto.UtcOffset),
                            CheckFollowUpAppointmentAsync(appointmentDto),
                            CheckPatientEmailAsync(appointmentDto.PatientId)
                        };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Validation failed for appointment");
                throw;
            }
        }

        private async Task CheckDoctorExistsAsync(Guid doctorId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var doctorExists = await context.DoctorDetails.AnyAsync(d => d.DoctorId == doctorId);
            if (!doctorExists)
            {
                throw new Exception("Doctor not found");
            }
        }

        private async Task CheckPatientExistsAsync(Guid patientId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var patientExists = await context.Users.AnyAsync(u => u.UserId == patientId);
            if (!patientExists)
            {
                throw new Exception("Patient not found");
            }
        }

        private async Task CheckDepartmentExistsAsync(Guid departmentId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var departmentExists = await context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
            if (!departmentExists)
            {
                throw new Exception("Department not found");
            }
        }

        private async Task CheckOverlappingAppointmentsAsync(AppointmentDTO appointmentDto)
        {
            await using var context = _contextFactory.CreateDbContext();
            var overlappingAppointment = await context.Appointments
                .AnyAsync(a => a.DoctorId == appointmentDto.DoctorId &&
                               a.AppointmentDate < appointmentDto.AppointmentDate.AddMinutes(30) &&
                               a.AppointmentDate > appointmentDto.AppointmentDate.AddMinutes(-30) &&
                               a.IsActive == true &&
                               a.AppointmentId != appointmentDto.AppointmentId);

            if (overlappingAppointment)
            {
                throw new Exception("The doctor has another appointment at the same time");
            }
        }

        private async Task CheckDoctorAvailabilityAsync(AppointmentDTO appointmentDto)
        {
            await using var context = _contextFactory.CreateDbContext();
            var doctorAvailability = await context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == appointmentDto.DoctorId &&
                                           da.AvailableFrom <= appointmentDto.AppointmentDate &&
                                           da.AvailableTo > appointmentDto.AppointmentDate &&
                                           da.IsAvailable == true &&
                                           da.IsSlotAvailable == true);

            if (doctorAvailability == null)
            {
                throw new Exception("The doctor is not available at the selected time");
            }
        }

        private Task CheckAppointmentTimeAsync(DateTime appointmentDate, TimeSpan utcOffset)
        {
            // Define allowed booking times in Local Time
            var startTime = new TimeSpan(7, 0, 0); // 7:00 AM Local Time
            var endTime = new TimeSpan(17, 0, 0); // 5:00 PM Local Time

            DateTime localAppointmentDate = TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset);

            if (localAppointmentDate.TimeOfDay < startTime || localAppointmentDate.TimeOfDay > endTime)
            {
                throw new InvalidOperationException("Appointments can only be booked between 7:00 AM and 5:00 PM local time.");
            }

            return Task.CompletedTask;
        }

        private async Task CheckFollowUpAppointmentAsync(AppointmentDTO appointmentDto)
        {
            if (appointmentDto.FollowUpAppointmentId.HasValue)
            {
                await using var context = _contextFactory.CreateDbContext();
                var followUpAppointment = await context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentDto.FollowUpAppointmentId.Value);

                if (followUpAppointment == null)
                {
                    throw new Exception("Follow-up appointment not found");
                }

                if (followUpAppointment.PatientId != appointmentDto.PatientId)
                {
                    throw new Exception("Follow-up appointment is not associated with the same patient");
                }
            }
        }

        private async Task CheckPatientEmailAsync(Guid patientId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var patient = await context.Users.FirstOrDefaultAsync(u => u.UserId == patientId);
            if (patient == null)
            {
                throw new Exception("Patient not found");
            }
            if (string.IsNullOrEmpty(patient.Email))
            {
                throw new Exception("Patient email is required to book an appointment.");
            }
        }


    }
}
