using DataTransferO;

public class DoctorDetailDTO
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Description { get; set; }
    public UserDTO User { get; set; } = new UserDTO();
    public DepartmentDTO? Department { get; set; }

    // Custom property to display the doctor's name
    public string DoctorName => User?.Name ?? "Unknown";
}
