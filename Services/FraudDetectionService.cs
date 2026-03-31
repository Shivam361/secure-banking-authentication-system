using Microsoft.EntityFrameworkCore;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBankingApp.Services
{
    public class FraudDetectionService : IFraudDetectionService
    {
        private readonly AppDbContext _db;

        public FraudDetectionService(AppDbContext db)
        {
            _db = db;
        }

        // Log every login attempt
        public void LogAttempt(string username, bool success, string ip)
        {
            var attempt = new LoginAttempt
            {
                Username = username,
                IPAddress = ip,
                IsSuccessful = success
            };
            _db.LoginAttempts.Add(attempt);
            _db.SaveChanges();

            // Simple rule: flag if 3+ failures in last 5 minutes from different IPs
            var window = DateTime.UtcNow.AddMinutes(-5);
            var failures = _db.LoginAttempts
                .Where(a => a.Username == username
                         && !a.IsSuccessful
                         && a.Timestamp >= window)
                .Select(a => a.IPAddress)
                .Distinct()
                .Count();

            if (failures >= 3)
            {
                _db.FraudLogs.Add(new FraudLog
                {
                    Description = $"Multiple failed logins ({failures}) for {username}",
                });
                _db.SaveChanges();
            }
        }
        public void ProcessTransaction(Transaction tx)
        {
            try
            {
                // Flag large transactions
                if (tx.Amount >= 500)
                {
                    _db.FraudLogs.Add(new FraudLog
                    {
                        Description = $"Large transaction: {tx.Amount:C} from {tx.FromUsername} to {tx.ToUsername} at {tx.Location}"
                    });
                    _db.SaveChanges();
                }


                // Example geo rule: location != "HomeTown"
                // Check sender's (from user) home location vs transaction location
                var sender = _db.Users.SingleOrDefault(u => u.Username == tx.FromUsername);
                // This assumes you have a LastLoginIP or HomeLocation property to compare to!
                var home = sender?.LastLoginIP; // or sender.HomeLocation if you added it
                                                // Only check if home is not null or empty
                if (!string.IsNullOrEmpty(home) &&
                    !string.Equals(tx.Location, home, StringComparison.OrdinalIgnoreCase))
                {
                    _db.FraudLogs.Add(new FraudLog
                    {
                        Description = $"Transaction at unusual location '{tx.Location}' for {tx.FromUsername}"
                    });
                    _db.SaveChanges();
                }
            }
            catch (DbUpdateException ex)
            {
                // Inspect the real database error:
                System.Diagnostics.Debug.WriteLine("DbUpdateException: " + ex.InnerException?.Message);
                throw;  // re-throw if you want the app to still crash, or handle gracefully
            }
        }
        public bool LocationsAreSimilar(string? locA, string? locB)
        {
            if (string.IsNullOrWhiteSpace(locA) || string.IsNullOrWhiteSpace(locB))
                return false;
            var a = locA.Trim().ToLowerInvariant();
            var b = locB.Trim().ToLowerInvariant();
            return a.Contains(b) || b.Contains(a);
        }
        public void LogFraud(string username, string? loginLocation, string? homeLocation)

        {
            _db.FraudLogs.Add(new FraudLog
            {
                Description = $"Login from unusual location '{loginLocation}' for user '{username}' (Home: {homeLocation})"
            });
            _db.SaveChanges();
        }
    }

    }

