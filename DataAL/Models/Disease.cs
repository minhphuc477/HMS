using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Disease
{
    public Guid DiseaseId { get; set; }

    public string DiseaseName { get; set; } = null!;

    public Guid? DepartmentId { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
