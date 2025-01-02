using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class PrescriptionDetail
{
    public Guid PrescriptionDetailId { get; set; }

    public Guid MedicalRecordId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public int DurationDays { get; set; }

    public string? Notes { get; set; }

    public virtual MedicalRecord MedicalRecord { get; set; } = null!;

    public virtual PharmacyProduct Product { get; set; } = null!;
}
