using System;
using System.Collections.Generic;

namespace DataAL.Models;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public Guid? OrderId { get; set; }

    public Guid? AppointmentId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
