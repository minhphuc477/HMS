using System;
using System.Collections.Generic;

namespace DataTransferO
{
    public class PharmacyProductDTO
    {
        public Guid ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool? RequiresPrescription { get; set; } 

        // Navigation properties
        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
        public ICollection<PrescriptionDetailDTO> PrescriptionDetails { get; set; } = new List<PrescriptionDetailDTO>();
    }
}
