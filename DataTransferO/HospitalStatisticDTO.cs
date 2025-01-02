using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferO
{
    public class HospitalStatisticDTO
    {
        public Guid StatisticId { get; set; }
        public DateOnly ReportDate { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
        public int TotalPatients { get; set; }
        public int NewPatients { get; set; }
        public int ReturningPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal? AppointmentIncome { get; set; }
        public decimal? PharmacyIncome { get; set; }
        public int? TotalMalePatients { get; set; }
        public int? TotalFemalePatients { get; set; }
        public int? TotalOtherGenderPatients { get; set; }
        public decimal? AveragePatientAge { get; set; }
        public int? TotalDoctors { get; set; }
        public int? AvailableDoctors { get; set; }
        public decimal? AverageWaitingTime { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
