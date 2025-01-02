using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Billing
{
    public Guid BillId { get; set; }

    public Guid? OrderId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? IssuedDate { get; set; }

    public DateTime DueDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid? PaymentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Payment? Payment { get; set; }
}
