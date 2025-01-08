using System;
using System.Collections.Generic;

namespace DataTransferO
{
    public class OrderDTO
    {
        public Guid OrderId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public Guid? PrescriptionId { get; set; }
        public decimal DoctorFee { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
        public DepartmentDTO? Department { get; set; }
    }
}
