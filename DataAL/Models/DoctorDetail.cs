using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class DoctorDetail
{
    public Guid DoctorId { get; set; }

    public Guid UserId { get; set; }

    public string? Specialization { get; set; }

    public string? Phone { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? Qualification { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public virtual User User { get; set; } = null!;
}
