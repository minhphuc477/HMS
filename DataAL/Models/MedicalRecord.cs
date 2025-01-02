using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class MedicalRecord
{
    public Guid MedicalRecordId { get; set; }

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public Guid? DiseaseId { get; set; }

    public string Diagnosis { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Disease? Disease { get; set; }

    public virtual DoctorDetail Doctor { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User Patient { get; set; } = null!;

    public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
