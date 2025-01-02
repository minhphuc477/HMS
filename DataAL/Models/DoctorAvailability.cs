using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class DoctorAvailability
{
    public Guid AvailabilityId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsSlotAvailable { get; set; } = true;
    public virtual DoctorDetail Doctor { get; set; } = null!;
    public TimeSpan UtcOffset { get; set; } // New property
}
