using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBankingApp.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public bool IsSuccessful { get; set; }
    }
}

