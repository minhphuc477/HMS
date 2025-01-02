using System;

namespace DataTransferO
{
    public class BillingDTO
    {
        public Guid BillId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Status { get; set; }
        public Guid? PaymentId { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        public OrderDTO? Order { get; set; }
        public PaymentDTO? Payment { get; set; }
    }
}

