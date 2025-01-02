using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Appointment
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? RescheduledAt { get; set; }
    public Guid? FollowUpAppointmentId { get; set; }
    public string Status { get; set; } = null!;
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;
    public virtual DoctorDetail Doctor { get; set; } = null!;
    public virtual Appointment? FollowUpAppointment { get; set; }
    public virtual ICollection<Appointment> InverseFollowUpAppointment { get; set; } = new List<Appointment>();
    public virtual User? Patient { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public TimeSpan UtcOffset { get; set; } // New property


}
