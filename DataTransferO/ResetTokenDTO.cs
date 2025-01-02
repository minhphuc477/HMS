using System;

namespace DataTransferO
{
    public class ResetTokenDTO
    {
        public Guid TokenId { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime Expiration { get; set; }
    }
}
