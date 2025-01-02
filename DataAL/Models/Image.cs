using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Image
{
    public Guid ImageId { get; set; }

    public Guid EntityId { get; set; }

    public string EntityType { get; set; } = null!;

    public byte[] ImageData { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
