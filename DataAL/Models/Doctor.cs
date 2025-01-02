using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Doctor
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Gender { get; set; } = null!;
}
