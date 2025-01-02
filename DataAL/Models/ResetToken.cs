using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class ResetToken
{
    public Guid TokenId { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime Expiration { get; set; }

    public virtual User User { get; set; } = null!;
}
