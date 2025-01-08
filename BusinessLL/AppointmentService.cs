using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; 
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class AppointmentService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;
        private readonly IAppointmentValidationService _validationService;
        private readonly IMapper _mapper;

        public AppointmentService(IDbContextFactory<HmsAContext> contextFactory, IAppointmentValidationService validationService, IMapper mapper)
        {
            _contextFactory = contextFactory;
            _validationService = validationService;
            _mapper = mapper;
        }

        #region Get Appointment Methods

        public async Task<AppointmentDTO?> GetAppointmentByIdAsync(Guid appointmentId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var appointment = await _context.Appointments
                .Include(a => a.Department)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .Include(a => a.FollowUpAppointment)
                .Include(a => a.InverseFollowUpAppointment)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            return appointment == null ? null : _mapper.Map<AppointmentDTO>(appointment);
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAllAppointmentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var appointments = await _context.Appointments
                .Include(a => a.Department)
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .Include(a => a.FollowUpAppointment)
                .Include(a => a.InverseFollowUpAppointment)
                .Include(a => a.Payments)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDTO>> GetUpcomingAppointmentsAsync(Guid patientId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var appointments = await context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Department)
                .Where(a => a.PatientId == patientId && a.AppointmentDate >= DateTime.Now)
                .ToListAsync();

            var filteredAppointments = appointments
                .Where(a => a.IsActive.GetValueOrDefault() && !a.IsDeleted.GetValueOrDefault())
                .ToList();

            return _mapper.Map<IEnumerable<AppointmentDTO>>(filteredAppointments);
        }

        public async Task<IEnumerable<AppointmentDTO>> GetPastAppointmentsAsync(Guid patientId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var filteredAppointments = await context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId
                            && a.AppointmentDate < DateTime.Now
                            && (a.IsActive ?? false)
                            && !(a.IsDeleted ?? false))
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Department)
                .ToListAsync();

            Console.WriteLine($"Fetched {filteredAppointments.Count} past appointments for PatientId: {patientId}");

            return _mapper.Map<IEnumerable<AppointmentDTO>>(filteredAppointments);
        }

        public async Task<AppointmentDTO> CreateAppointmentAsync(
            Guid doctorId,
            Guid patientId,
            Guid departmentId,
            DateTime localAppointmentDate,
            TimeSpan utcOffset,
            string notes,
            Guid? followUpAppointmentId = null)
        {
            Log.Information("Creating appointment with DoctorId: {DoctorId}, PatientId: {PatientId}, DepartmentId: {DepartmentId}, AppointmentDate: {AppointmentDate}",
                doctorId, patientId, departmentId, localAppointmentDate);

            // Convert local time to UTC
            var appointmentDateUtc = TimeZoneHelper.ConvertLocalToUtc(localAppointmentDate, TimeSpan.Zero);

            var startTime = new TimeSpan(7, 0, 0);
            var endTime = new TimeSpan(17, 0, 0);
            if (localAppointmentDate.TimeOfDay < startTime || localAppointmentDate.TimeOfDay > endTime)
            {
                throw new InvalidOperationException("Appointments can only be booked between 7:00 AM and 5:00 PM local time.");
            }

            var isSlotAvailable = await IsSlotAvailableAsync(doctorId, appointmentDateUtc);
            if (!isSlotAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            var appointmentDto = new AppointmentDTO
            {
                AppointmentId = Guid.NewGuid(),
                DoctorId = doctorId,
                PatientId = patientId,
                DepartmentId = departmentId,
                AppointmentDate = appointmentDateUtc,
                Notes = notes,
                FollowUpAppointmentId = followUpAppointmentId,
                Status = "Pending",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UtcOffset = TimeSpan.Zero
            };

            try
            {
                await _validationService.ValidateAppointmentAsync(appointmentDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Validation failed for appointment");
                throw;
            }

            await using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var patient = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == patientId);

                if (patient == null)
                {
                    Log.Error("Patient not found with PatientId: {PatientId}", patientId);
                    throw new Exception("Patient not found");
                }

                Log.Information("Fetched patient with PatientId: {PatientId}, Email: {Email}", patient.UserId, patient.Email);

                if (string.IsNullOrEmpty(patient.Email))
                {
                    Log.Error("Patient email is required to book an appointment. PatientId: {PatientId}", patientId);
                    throw new Exception("Patient email is required to book an appointment.");
                }

                // Detach the patient entity to prevent EF Core from tracking it
                context.Entry(patient).State = EntityState.Detached;

                var appointment = _mapper.Map<Appointment>(appointmentDto);
                appointment.PatientId = patientId;
                appointment.Patient = null;
                appointment.Doctor = null;
                appointment.Department = null;

                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();

                await MarkSlotAsUnavailableAsync(doctorId, appointmentDateUtc);

                await transaction.CommitAsync();

                Log.Information("Appointment created successfully with ID: {AppointmentId}", appointment.AppointmentId);
                return _mapper.Map<AppointmentDTO>(appointment);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Transaction failed, rolling back. Inner exception: {InnerException}", ex.InnerException?.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AppointmentDTO> UpdateAppointmentAsync(AppointmentDTO appointmentDto)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var existingAppointment = await _context.Appointments.FindAsync(appointmentDto.AppointmentId);
            if (existingAppointment == null)
            {
                throw new Exception("Appointment not found");
            }

            await _validationService.ValidateAppointmentAsync(appointmentDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _mapper.Map(appointmentDto, existingAppointment);
                existingAppointment.RescheduledAt = DateTime.Now;

                _context.Appointments.Update(existingAppointment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return _mapper.Map<AppointmentDTO>(existingAppointment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AppointmentDTO> RescheduleAppointmentAsync(Guid appointmentId, Guid doctorId, DateTime newLocalAppointmentDate)
        {
            if (newLocalAppointmentDate < DateTime.Now)
            {
                throw new ArgumentException("Cannot reschedule an appointment to a past date.", nameof(newLocalAppointmentDate));
            }

            var newAppointmentDate = TimeZoneHelper.ConvertLocalToUtc(newLocalAppointmentDate, TimeSpan.FromHours(0)); // Assuming UTC+0 for simplicity

            if (!await IsSlotAvailableAsync(doctorId, newAppointmentDate))
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            await using var _context = _contextFactory.CreateDbContext();
            var appointment = await _context.Appointments
                .Include(a => a.Patient) // Include patient details
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            var oldAppointmentDate = appointment.AppointmentDate;

            appointment.AppointmentDate = newAppointmentDate;
            appointment.RescheduledAt = DateTime.Now;

            await _validationService.ValidateAppointmentAsync(_mapper.Map<AppointmentDTO>(appointment));

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            // Mark the new slot as unavailable
            await MarkSlotAsUnavailableAsync(doctorId, newAppointmentDate);

            // Mark the old slot as available
            await MarkSlotAsAvailableAsync(doctorId, oldAppointmentDate);

            return _mapper.Map<AppointmentDTO>(appointment);
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDepartmentAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty)
                throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var appointments = await _context.Appointments
                .Include(a => a.Patient) // Include patient information
                .Where(a => a.DepartmentId == departmentId && a.IsActive == true && a.IsDeleted == false)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }

        #endregion

        #region Delete and Search Appointment Methods

        public async Task DeleteAppointmentAsync(Guid appointmentId)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            appointment.IsDeleted = true;
            appointment.IsActive = false;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AppointmentDTO>> SearchAppointmentsAsync(Guid? doctorId, Guid? patientId, Guid? departmentId, DateTime? startDate, DateTime? endDate)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var query = _context.Appointments.AsQueryable();

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            if (patientId.HasValue)
            {
                query = query.Where(a => a.PatientId == patientId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            var appointments = await query.ToListAsync();
            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }

        public async Task<AppointmentDTO> UpdateAppointmentStatusAsync(Guid appointmentId, string status)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var appointment = await _context.Appointments
                .Include(a => a.Patient) // Include patient details
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            appointment.Status = status;
            if (status == "Canceled")
            {
                appointment.IsActive = false;
            }

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return _mapper.Map<AppointmentDTO>(appointment);
        }

        private List<DateTime> GenerateTimeSlots(DateTime availableFrom, DateTime availableTo, TimeSpan slotDuration)
        {
            var slots = new List<DateTime>();
            for (var time = availableFrom; time < availableTo; time = time.Add(slotDuration))
            {
                slots.Add(time);
            }
            return slots;
        }

        public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(Guid doctorId, DateTime appointmentDate)
        {
            await using var context = _contextFactory.CreateDbContext();

            var availabilities = await context.DoctorAvailabilities
                .Where(a => a.DoctorId == doctorId && a.AvailableFrom.Date == appointmentDate.Date)
                .ToListAsync();

            if (!availabilities.Any())
            {
                return new List<DateTime>();
            }

            var availableTimeSlots = new List<DateTime>();

            foreach (var availability in availabilities)
            {
                var timeSlots = GenerateTimeSlots(availability.AvailableFrom, availability.AvailableTo, TimeSpan.FromMinutes(30));

                var existingAppointments = await context.Appointments
                    .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == appointmentDate.Date)
                    .Select(a => a.AppointmentDate)
                    .ToListAsync();

                var availableSlots = timeSlots
                    .Where(slot => !existingAppointments.Contains(slot))
                    .ToList();

                availableTimeSlots.AddRange(availableSlots);
            }

            // Convert available slots to local time before returning
            var utcOffset = TimeSpan.Zero; // Vietnam time zone (UTC+7)
            return availableTimeSlots.OrderBy(slot => slot).Select(slot => TimeZoneHelper.ConvertUtcToLocal(slot, utcOffset)).ToList();
        }

        private async Task<bool> IsSlotAvailableAsync(Guid doctorId, DateTime slot)
        {
            await using var context = _contextFactory.CreateDbContext();

            var isAvailable = await context.DoctorAvailabilities.AnyAsync(da =>
                da.DoctorId == doctorId &&
                da.AvailableFrom <= slot &&
                da.AvailableTo > slot &&
                da.IsAvailable &&
                da.IsSlotAvailable);

            var isBooked = await context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == slot &&
                a.IsActive == true &&
                a.IsDeleted == false);

            Log.Information("Slot availability check for DoctorId {DoctorId} at {Slot}: IsAvailable={IsAvailable}, IsBooked={IsBooked}", doctorId, slot, isAvailable, isBooked);

            return isAvailable && !isBooked;
        }

        private async Task MarkSlotAsUnavailableAsync(Guid doctorId, DateTime appointmentDateTime)
        {
            await using var context = _contextFactory.CreateDbContext();

            var availability = await context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
                da.DoctorId == doctorId &&
                da.AvailableFrom <= appointmentDateTime &&
                da.AvailableTo > appointmentDateTime);

            if (availability != null)
            {
                availability.IsSlotAvailable = false;
                context.DoctorAvailabilities.Update(availability);
                await context.SaveChangesAsync();
            }
        }

        private async Task MarkSlotAsAvailableAsync(Guid doctorId, DateTime appointmentDateTime)
        {
            await using var context = _contextFactory.CreateDbContext();

            var availability = await context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
                da.DoctorId == doctorId &&
                da.AvailableFrom <= appointmentDateTime &&
                da.AvailableTo > appointmentDateTime);

            if (availability != null)
            {
                availability.IsSlotAvailable = true;
                context.DoctorAvailabilities.Update(availability);
                await context.SaveChangesAsync();
            }
        }

        private async Task MarkOtherSlotsAsAvailableAsync(Guid doctorId, DateTime newAppointmentDate)
        {
            await using var _context = _contextFactory.CreateDbContext();

            var availability = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
                da.DoctorId == doctorId &&
                da.AvailableFrom <= newAppointmentDate &&
                da.AvailableTo > newAppointmentDate);

            if (availability != null)
            {
                availability.IsSlotAvailable = false;
                _context.DoctorAvailabilities.Update(availability);

                var otherAvailabilities = await _context.DoctorAvailabilities
                    .Where(da => da.DoctorId == doctorId && da.AvailableFrom != newAppointmentDate)
                    .ToListAsync();

                foreach (var otherAvailability in otherAvailabilities)
                {
                    otherAvailability.IsSlotAvailable = true;
                    _context.DoctorAvailabilities.Update(otherAvailability);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<AppointmentDTO>> GetConfirmedAppointmentsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();
            var confirmedAppointments = await context.Appointments
                .Where(a => a.Status == "Confirmed")
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Department)
                .ToListAsync();

            return _mapper.Map<List<AppointmentDTO>>(confirmedAppointments);
        }

        public async Task<AppointmentDTO> MarkAppointmentAsCompleteAsync(Guid appointmentId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var appointment = await context.Appointments
                .Include(a => a.Patient) // Include patient details
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            appointment.Status = "Completed";
            appointment.IsActive = false;

            context.Appointments.Update(appointment);
            await context.SaveChangesAsync();

            return _mapper.Map<AppointmentDTO>(appointment);
        }





        #endregion
    }
}

