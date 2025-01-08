namespace DataTransferO
{
    public class DoctorGridViewDTO
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string Specialization { get; set; }
        public string? Qualification { get; set; }
        public int? ExperienceYears { get; set; }
        public string Description { get; set; }
        public string? DepartmentName { get; set; }
        public DepartmentDTO? Department { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
