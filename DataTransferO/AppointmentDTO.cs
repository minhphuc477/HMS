using DataTransferO;

public class AppointmentDTO
{
    public Guid AppointmentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? RescheduledAt { get; set; }
    public Guid? FollowUpAppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public TimeSpan UtcOffset { get; set; } // New property

    public UserDTO? Patient { get; set; } // Add this property

    // Navigation properties
    public DepartmentDTO Department { get; set; } = new DepartmentDTO();
    public DoctorDetailDTO Doctor { get; set; } = new DoctorDetailDTO();



    // New property for formatted date
    public string FormattedAppointmentDate => AppointmentDate.ToString("f");


    // new property to use for create the pdf format information 
    public string FormattedAppointmentDateForPDF => AppointmentDate.ToString("yyyy-MM-dd HH:mm");
}
