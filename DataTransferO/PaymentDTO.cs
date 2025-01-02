using System;
using System.Collections.Generic;

namespace DataTransferO
{
    public class PaymentDTO
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; } 
        public string? PaymentStatus { get; set; } 
        public Guid? OrderId { get; set; }
        public Guid? AppointmentId { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}


