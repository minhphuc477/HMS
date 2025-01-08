using System;

namespace DataTransferO
{
    public class OrderDetailDTO
    {
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Navigation properties
        public PharmacyProductDTO Product { get; set; } = null!;
    }
}
