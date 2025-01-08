using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class DoctorService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;
        private readonly IMapper _mapper;

        public DoctorService(IDbContextFactory<HmsAContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        #region Doctor Availability

        public async Task<IEnumerable<DoctorAvailabilityDTO>> GetDoctorAvailabilityAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty) throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var _context = _contextFactory.CreateDbContext();
            var availabilities = await _context.DoctorAvailabilities
                .Where(da => da.DoctorId == doctorId && da.IsAvailable == true)
                .ToListAsync();

            // Set DateTimeKind for each availability
            availabilities.ForEach(availability =>
            {
                availability.AvailableFrom = DateTime.SpecifyKind(availability.AvailableFrom, DateTimeKind.Utc);
                availability.AvailableTo = DateTime.SpecifyKind(availability.AvailableTo, DateTimeKind.Utc);
            });

            return _mapper.Map<IEnumerable<DoctorAvailabilityDTO>>(availabilities);
        }

        public async Task<bool> IsDoctorAvailableAsync(Guid doctorId, DateTime dateTime)
        {
            if (doctorId == Guid.Empty) throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var _context = _contextFactory.CreateDbContext();
            var availability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId
                                           && da.IsAvailable == true
                                           && da.IsSlotAvailable == true
                                           && da.AvailableFrom <= dateTime
                                           && da.AvailableTo > dateTime);

            return availability != null;
        }

        public async Task AddOrUpdateDoctorAvailabilityAsync(DoctorAvailabilityDTO availabilityDto)
        {
            if (availabilityDto == null) throw new ArgumentNullException(nameof(availabilityDto));

            await using var _context = _contextFactory.CreateDbContext();
            var existingAvailability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.AvailabilityId == availabilityDto.AvailabilityId);

            if (existingAvailability != null)
            {
                _mapper.Map(availabilityDto, existingAvailability);
                _context.DoctorAvailabilities.Update(existingAvailability);
            }
            else
            {
                var newAvailability = _mapper.Map<DoctorAvailability>(availabilityDto);
                newAvailability.AvailabilityId = Guid.NewGuid();
                _context.DoctorAvailabilities.Add(newAvailability);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctorAvailabilityAsync(Guid availabilityId)
        {
            if (availabilityId == Guid.Empty) throw new ArgumentException("Availability ID cannot be empty.", nameof(availabilityId));

            await using var _context = _contextFactory.CreateDbContext();
            var availability = await _context.DoctorAvailabilities.FindAsync(availabilityId);
            if (availability == null)
            {
                throw new KeyNotFoundException("Doctor availability not found.");
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
        }

        public async Task SetDailyAvailabilityAsync(Guid doctorId, TimeSpan startTime, TimeSpan endTime, TimeSpan slotDuration, int utcOffset)
        {
            if (doctorId == Guid.Empty) throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var context = _contextFactory.CreateDbContext();

            for (int i = 0; i < 7; i++) // Loop through each day for the next 7 days
            {
                var localDate = DateTime.Now.Date.AddDays(i);

                var availableFrom = localDate.Add(startTime);
                var availableTo = localDate.Add(endTime);

                if (availableFrom < DateTime.UtcNow)
                {
                    continue;
                }

                var timeSlots = GenerateTimeSlots(availableFrom, availableTo, slotDuration);

                foreach (var slot in timeSlots)
                {
                    var existingAvailability = await context.DoctorAvailabilities
                        .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.AvailableFrom == slot);

                    if (existingAvailability == null)
                    {
                        var newAvailability = new DoctorAvailability
                        {
                            AvailabilityId = Guid.NewGuid(),
                            DoctorId = doctorId,
                            AvailableFrom = slot.ToUniversalTime(),
                            AvailableTo = slot.Add(slotDuration).ToUniversalTime(),
                            IsAvailable = true,
                            IsSlotAvailable = true,
                            UtcOffset = TimeSpan.Zero
                        };
                        context.DoctorAvailabilities.Add(newAvailability);
                    }
                }
            }

            await context.SaveChangesAsync();
            Log.Information("Set daily availability for doctor {DoctorId} from {StartTime} to {EndTime} with slot duration {SlotDuration} for the next 7 days", doctorId, startTime, endTime, slotDuration);
        }

        private List<DateTime> GenerateTimeSlots(DateTime availableFrom, DateTime availableTo, TimeSpan slotDuration)
        {
            var slots = new List<DateTime>();
            for (var time = availableFrom; time < availableTo; time = time.Add(slotDuration))
            {
                if (time.TimeOfDay >= new TimeSpan(7, 0, 0) && time.TimeOfDay <= new TimeSpan(17, 0, 0))
                {
                    slots.Add(time);
                }
            }
            return slots;
        }

        public async Task ClearOldAvailabilityAsync(Guid doctorId, DateTime cutoffDate)
        {
            await using var _context = _contextFactory.CreateDbContext();
            var oldAvailabilities = await _context.DoctorAvailabilities
                .Where(da => da.DoctorId == doctorId && da.AvailableFrom < cutoffDate)
                .ToListAsync();

            _context.DoctorAvailabilities.RemoveRange(oldAvailabilities);
            await _context.SaveChangesAsync();
        }

        public async Task GenerateAvailabilityForAllDoctorsAsync(TimeSpan startTime, TimeSpan endTime, TimeSpan slotDuration, int utcOffset)
        {
            await using var context = _contextFactory.CreateDbContext();

            var activeDoctors = await context.DoctorDetails
                .Include(d => d.User)
                .Where(d => d.User.IsActive == true && d.User.IsDeleted == false)
                .ToListAsync();

            foreach (var doctor in activeDoctors)
            {
                for (int i = 0; i < 7; i++) // Loop through each day for the next 7 days
                {
                    var localDate = DateTime.UtcNow.Date.AddDays(i);

                    var availableFrom = localDate.Add(startTime);
                    var availableTo = localDate.Add(endTime);

                    if (availableFrom < DateTime.UtcNow)
                    {
                        continue;
                    }

                    var timeSlots = GenerateTimeSlots(availableFrom, availableTo, slotDuration);

                    foreach (var slot in timeSlots)
                    {
                        var existingAvailability = await context.DoctorAvailabilities
                            .FirstOrDefaultAsync(da => da.DoctorId == doctor.DoctorId && da.AvailableFrom == slot);

                        if (existingAvailability == null)
                        {
                            var newAvailability = new DoctorAvailability
                            {
                                AvailabilityId = Guid.NewGuid(),
                                DoctorId = doctor.DoctorId,
                                AvailableFrom = slot, //UTC can be add here
                                AvailableTo = slot.Add(slotDuration),
                                IsAvailable = true,
                                IsSlotAvailable = true,
                                UtcOffset = TimeSpan.Zero
                            };
                            context.DoctorAvailabilities.Add(newAvailability);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            Log.Information("Generated availability for all doctors from {StartTime} to {EndTime} with slot duration {SlotDuration} for the next 7 days", startTime, endTime, slotDuration);
        }

        #endregion

        #region Doctor Management

        public async Task<IEnumerable<DoctorDetailDTO>> GetDoctorsByDepartmentAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty) throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var doctors = await _context.DoctorDetails
                .Include(d => d.User)
                .Where(d => d.DepartmentId == departmentId && d.User.IsActive == true)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDetailDTO>>(doctors);
        }

        public async Task<IEnumerable<DoctorDetailDTO>> GetAllDoctorsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var doctors = await _context.DoctorDetails
                .Include(d => d.User)
                .Include(d => d.Department)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDetailDTO>>(doctors);
        }

        public async Task<IEnumerable<DoctorDetailDTO>> GetAllActiveDoctorsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var doctors = await _context.DoctorDetails
                .Include(d => d.User)
                .Include(d => d.Department)
                .Where(d => d.User.IsActive == true && d.User.IsDeleted == false)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorDetailDTO>>(doctors);
        }

        public async Task<DoctorDetailDTO> GetDoctorByIdAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty) throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var _context = _contextFactory.CreateDbContext();
            var doctor = await _context.DoctorDetails
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            return _mapper.Map<DoctorDetailDTO>(doctor);
        }

        public async Task<DoctorDetailDTO> GetDoctorByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            await using var _context = _contextFactory.CreateDbContext();
            var doctor = await _context.DoctorDetails
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            return _mapper.Map<DoctorDetailDTO>(doctor);
        }

        public async Task<DoctorDetailDTO> CreateDoctorAsync(DoctorDetailDTO doctorDto)
        {
            using var context = _contextFactory.CreateDbContext();
            var doctorDetail = _mapper.Map<DoctorDetail>(doctorDto);

            // Ensure the User property is set
            doctorDetail.User = await context.Users.FindAsync(doctorDto.UserId);
            doctorDetail.Phone = doctorDto.Phone; // Ensure the PhoneNumber is set

            context.DoctorDetails.Add(doctorDetail);
            await context.SaveChangesAsync();

            return _mapper.Map<DoctorDetailDTO>(doctorDetail);
        }

        public async Task<DoctorDetailDTO> UpdateDoctorAsync(DoctorDetailDTO doctorDto)
        {
            if (doctorDto == null) throw new ArgumentNullException(nameof(doctorDto));

            await using var _context = _contextFactory.CreateDbContext();
            var existingDoctor = await _context.DoctorDetails.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == doctorDto.DoctorId);
            if (existingDoctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            // Ensure the User exists before updating DoctorDetail
            var user = await _context.Users.FindAsync(doctorDto.UserId);
            if (user == null)
            {
                throw new Exception("User not found. Please ensure the user exists before updating the doctor.");
            }

            // Update the User details if necessary
            existingDoctor.User.Name = doctorDto.User.Name;
            existingDoctor.User.Email = doctorDto.User.Email;
            existingDoctor.User.PhoneNumber = doctorDto.User.PhoneNumber;
            existingDoctor.User.DateOfBirth = doctorDto.User.DateOfBirth;
            existingDoctor.User.Gender = doctorDto.User.Gender;

            // Update the Doctor details
            _mapper.Map(doctorDto, existingDoctor);
            _context.DoctorDetails.Update(existingDoctor);
            await _context.SaveChangesAsync();
            return _mapper.Map<DoctorDetailDTO>(existingDoctor);
        }

        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty) throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var _context = _contextFactory.CreateDbContext();
            var doctor = await _context.DoctorDetails.FindAsync(doctorId);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found.");
            }

            _context.DoctorDetails.Remove(doctor);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetPatientCountForDepartmentAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty) throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var doctors = await _context.DoctorDetails
                .Where(d => d.DepartmentId == departmentId && d.User.IsActive == true && d.User.IsDeleted == false)
                .ToListAsync();

            int patientCount = 0;
            foreach (var doctor in doctors)
            {
                var patients = await _context.Appointments
                    .Where(a => a.DoctorId == doctor.DoctorId && a.IsActive == true && a.IsDeleted == false)
                    .Select(a => a.Patient)
                    .Distinct()
                    .CountAsync();

                patientCount += patients;
            }

            return patientCount;
        }


        public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsForDoctorAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty)
                throw new ArgumentException("Doctor ID cannot be empty.", nameof(doctorId));

            await using var _context = _contextFactory.CreateDbContext();
            var appointments = await _context.Appointments
                .Include(a => a.Patient) // Include patient information
                .Where(a => a.DoctorId == doctorId && a.IsActive == true && a.IsDeleted == false)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
        }

        public async Task<Dictionary<Guid, DateTime>> GetAppointmentDatesAsync(Guid doctorId)
        {
            var appointments = await GetAppointmentsForDoctorAsync(doctorId);
            return appointments
                .GroupBy(a => a.PatientId)
                .ToDictionary(g => g.Key, g => g.Max(a => a.AppointmentDate));
        }

        public async Task<DiseaseDTO> GetDiseaseByIdAsync(Guid diseaseId)
        {
            if (diseaseId == Guid.Empty) throw new ArgumentException("Disease ID cannot be empty.", nameof(diseaseId));

            await using var _context = _contextFactory.CreateDbContext();
            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.DiseaseId == diseaseId);

            if (disease == null)
            {
                throw new KeyNotFoundException("Disease not found.");
            }

            return _mapper.Map<DiseaseDTO>(disease);
        }

        public async Task AddDiseaseToDepartmentAsync(DiseaseDTO diseaseDto)
        {
            if (diseaseDto == null) throw new ArgumentNullException(nameof(diseaseDto));

            await using var _context = _contextFactory.CreateDbContext();
            var disease = _mapper.Map<Disease>(diseaseDto);
            _context.Diseases.Add(disease);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DiseaseDTO>> GetDiseasesByDepartmentAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty) throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var diseases = await _context.Diseases
                .Where(d => d.DepartmentId == departmentId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DiseaseDTO>>(diseases);
        }

        #endregion

        #region Department Management and Disease Management

        public async Task<IEnumerable<DepartmentDTO>> GetDepartmentsAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var departments = await _context.Departments.ToListAsync();
            return _mapper.Map<IEnumerable<DepartmentDTO>>(departments);
        }

        public async Task<IEnumerable<DepartmentDTO>> GetDepartmentsDesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var departments = await context.Departments
                .Include(d => d.Diseases)
                .ToListAsync();
            return _mapper.Map<IEnumerable<DepartmentDTO>>(departments);
        }

        public async Task<IEnumerable<DiseaseDTO>> GetAllDiseasesAsync()
        {
            await using var _context = _contextFactory.CreateDbContext();
            var diseases = await _context.Diseases.ToListAsync();
            return _mapper.Map<IEnumerable<DiseaseDTO>>(diseases);
        }

        public async Task AddDepartmentAsync(DepartmentDTO departmentDto)
        {
            if (departmentDto == null) throw new ArgumentNullException(nameof(departmentDto));

            await using var _context = _contextFactory.CreateDbContext();
            var department = _mapper.Map<Department>(departmentDto);
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDepartmentAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty) throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiseaseAsync(Guid diseaseId)
        {
            if (diseaseId == Guid.Empty) throw new ArgumentException("Disease ID cannot be empty.", nameof(diseaseId));

            await using var _context = _contextFactory.CreateDbContext();
            var disease = await _context.Diseases.FindAsync(diseaseId);
            if (disease == null)
            {
                throw new KeyNotFoundException("Disease not found.");
            }

            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDepartmentAndDiseasesAsync(Guid departmentId)
        {
            if (departmentId == Guid.Empty) throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

            await using var _context = _contextFactory.CreateDbContext();
            var department = await _context.Departments
                .Include(d => d.Diseases)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

            if (department == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

            // Delete associated diseases
            _context.Diseases.RemoveRange(department.Diseases);

            // Delete department
            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
