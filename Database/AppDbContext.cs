using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using SecureBankingApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SecureBankingApp.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<SecureBankingApp.Models.User> Users
            => Set<SecureBankingApp.Models.User>();
        public DbSet<SecureBankingApp.Models.LoginAttempt> LoginAttempts
            => Set<SecureBankingApp.Models.LoginAttempt>();
        public DbSet<SecureBankingApp.Models.FraudLog> FraudLogs
            => Set<SecureBankingApp.Models.FraudLog>();

        public DbSet<OTPRequest> OTPRequests => Set<OTPRequest>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        private static bool _created;
        public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
        {
            if (!_created)
            {
                _created = true;
                Database.EnsureCreated();
            }
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "banking.db");
            options.UseSqlite($"Data Source={path};Password=SuperSecretBankKey2026!");
        }
    }
}


