using System;

namespace DataTransferO
{
    public class ImageDTO
    {
        public Guid ImageId { get; set; }
        public Guid EntityId { get; set; }
        public string? EntityType { get; set; }
        public byte[]? ImageData { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Optional: Add a constructor to initialize from an Image entity
        public ImageDTO() { }
    }
}
