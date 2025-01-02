public class DoctorAvailabilityDTO
{
    public Guid AvailabilityId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsSlotAvailable { get; set; }
    public TimeSpan UtcOffset { get; set; } // New property
}
