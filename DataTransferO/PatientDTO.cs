using System;

namespace DataTransferO
{
    public class PatientDTO
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
    }
}
