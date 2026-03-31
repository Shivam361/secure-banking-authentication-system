using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBankingApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? LastLoginIP { get; set; }
        public string? HomeLocation { get; set; } //for geo fraud
        public decimal Balance { get; set; } = 0.0m;

        /// <summary>The user's role — determines what features and data they can access.</summary>
        public UserRole Role { get; set; } = UserRole.Customer;

        /// <summary>Convenience property — true when the user holds the Admin role.</summary>
        public bool IsAdmin => Role == UserRole.Admin;

        public int FailedLoginCount { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; } // When the account becomes available again
    }
}



