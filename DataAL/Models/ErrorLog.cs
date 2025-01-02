using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class ErrorLog
{
    public Guid ErrorLogId { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? ErrorDateTime { get; set; }
}
