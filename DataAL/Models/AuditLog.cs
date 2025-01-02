using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class AuditLog
{
    public Guid AuditId { get; set; }

    public string? TableName { get; set; }

    public Guid? RecordId { get; set; }

    public string? ChangeType { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime? ChangeDate { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}
