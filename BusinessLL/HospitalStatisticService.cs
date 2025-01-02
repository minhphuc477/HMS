using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class HospitalStatisticService
    {
        private readonly IDbContextFactory<HmsAContext> _contextFactory;
        private readonly IMapper _mapper;

        public HospitalStatisticService(IDbContextFactory<HmsAContext> contextFactory, IMapper mapper)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<HospitalStatisticDTO> CalculateStatisticsAsync(Guid doctorId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var totalAppointments = await context.Appointments
                .CountAsync(a => a.DoctorId == doctorId && a.IsActive == true && a.IsDeleted == false);

            var canceledAppointments = await context.Appointments
                .CountAsync(a => a.DoctorId == doctorId && a.Status == "Canceled" && a.IsActive == false);

            var newAppointments = await context.Appointments
                .CountAsync(a => a.DoctorId == doctorId && a.FollowUpAppointmentId == null && a.IsActive == true && a.IsDeleted == false);

            var followUpAppointments = await context.Appointments
                .CountAsync(a => a.DoctorId == doctorId && a.FollowUpAppointmentId != null && a.IsActive == true && a.IsDeleted == false);

            var ratioNewToFollowUp = followUpAppointments == 0 ? 0 : (double)newAppointments / followUpAppointments;

            Log.Information("DoctorId: {DoctorId}, TotalAppointments: {TotalAppointments}, CanceledAppointments: {CanceledAppointments}, NewAppointments: {NewAppointments}, FollowUpAppointments: {FollowUpAppointments}, RatioNewToFollowUp: {RatioNewToFollowUp}",
                doctorId, totalAppointments, canceledAppointments, newAppointments, followUpAppointments, ratioNewToFollowUp);

            var hospitalStatistic = new HospitalStatisticDTO
            {
                StatisticId = Guid.NewGuid(),
                ReportDate = DateOnly.FromDateTime(DateTime.Now),
                ReportMonth = DateTime.Now.Month,
                ReportYear = DateTime.Now.Year,
                TotalAppointments = totalAppointments,
                CanceledAppointments = canceledAppointments,
                NewPatients = newAppointments,
                ReturningPatients = followUpAppointments,
                TotalIncome = 0, // Placeholder, calculate as needed
                AppointmentIncome = 0, // Placeholder, calculate as needed
                PharmacyIncome = 0, // Placeholder, calculate as needed
                TotalMalePatients = 0, // Placeholder, calculate as needed
                TotalFemalePatients = 0, // Placeholder, calculate as needed
                TotalOtherGenderPatients = 0, // Placeholder, calculate as needed
                AveragePatientAge = 0, // Placeholder, calculate as needed
                TotalDoctors = 0, // Placeholder, calculate as needed
                AvailableDoctors = 0, // Placeholder, calculate as needed
                AverageWaitingTime = 0, // Placeholder, calculate as needed
                CreatedAt = DateTime.Now
            };

            return hospitalStatistic;
        }
    }
}
