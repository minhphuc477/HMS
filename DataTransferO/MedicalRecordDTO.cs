using System;
using System.Collections.Generic;

namespace DataTransferO
{
    public class MedicalRecordDTO
    {
        public Guid MedicalRecordId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public string? Diagnosis { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

