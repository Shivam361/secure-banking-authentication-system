using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SecureBankingApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string FromUsername { get; set; } = null!; // sender
        public string ToUsername { get; set; } = null!;   // receiver
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Location { get; set; }
        public string? Reference { get; set; }
    }

}


