using System;

namespace SecureBankingApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Action { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
