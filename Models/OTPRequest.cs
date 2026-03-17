using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBankingApp.Models
{
    public class OTPRequest
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Code { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}

