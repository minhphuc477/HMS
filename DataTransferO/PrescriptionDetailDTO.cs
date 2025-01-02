using System;

namespace DataTransferO
{
    public class PrescriptionDetailDTO
    {
        public Guid PrescriptionDetailId { get; set; }
        public Guid MedicalRecordId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int DurationDays { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public MedicalRecordDTO MedicalRecord { get; set; } = null!;
        public PharmacyProductDTO Product { get; set; } = null!;
    }
}


