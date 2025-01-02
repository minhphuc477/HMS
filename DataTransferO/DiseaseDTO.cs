using System;
using System.Collections.Generic;

namespace DataTransferO
{
    public class DiseaseDTO
    {
        public Guid DiseaseId { get; set; }
        public string DiseaseName { get; set; } = null!;
        public Guid? DepartmentId { get; set; }

        // Navigation properties
        public DepartmentDTO? Department { get; set; }
        public ICollection<MedicalRecordDTO> MedicalRecords { get; set; } = new List<MedicalRecordDTO>();
    }
}


