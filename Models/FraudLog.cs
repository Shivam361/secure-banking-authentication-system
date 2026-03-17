using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBankingApp.Models
{
    public class FraudLog
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}

