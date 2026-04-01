using Microsoft.EntityFrameworkCore;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;

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
                // Local ML-Based Fraud Detection System (Feature 5.6)
                // Use historical transaction values to detect statistically rare spikes
                var context = new Microsoft.ML.MLContext();

                // Fetch a sliding window of historical transactions to establish baseline
                var historicalTxs = _db.Transactions
                    .Where(t => t.FromUsername == tx.FromUsername)
                    .OrderBy(t => t.Timestamp)
                    .Select(t => new TransactionData { Amount = (float)t.Amount })
                    .ToList();

                // Add current incoming transaction
                historicalTxs.Add(new TransactionData { Amount = (float)tx.Amount });

                if (historicalTxs.Count >= 5) // Minimum sample to estimate IID
                {
                    try 
                    {
                        var dataView = context.Data.LoadFromEnumerable(historicalTxs);
                        
                        // IidSpikeEstimator checks for independent spikes out of 99% confidence bounds
                        var estimator = context.Transforms.DetectIidSpike(
                            outputColumnName: nameof(TransactionPrediction.Prediction), 
                            inputColumnName: nameof(TransactionData.Amount), 
                            confidence: 99.0, 
                            pvalueHistoryLength: Math.Min(historicalTxs.Count / 2, 20)); // dynamically scaled history

                        var transformer = estimator.Fit(dataView);
                        var transformedData = transformer.Transform(dataView);
                        
                        var predictions = context.Data.CreateEnumerable<TransactionPrediction>(transformedData, reuseRowObject: false).ToList();
                        
                        // The last prediction maps to our newest transaction
                        var latestPrediction = predictions.Last();
                        if (latestPrediction.Prediction != null && latestPrediction.Prediction[0] == 1) // 1 = Anomaly Spike Detected
                        {
                            _db.FraudLogs.Add(new FraudLog
                            {
                                Description = $"ML ALERT: Anomalous Spike detected. {tx.Amount:C} vastly exceeds typical spending patterns for {tx.FromUsername}."
                            });
                        }
                    }
                    catch { } // Handle ML matrix fitting errors silently on edge cases
                }
                else if (tx.Amount >= 500)
                {
                    // Fallback to static rule if not enough ML history is present
                    _db.FraudLogs.Add(new FraudLog
                    {
                        Description = $"Large transaction (ML warming up): {tx.Amount:C} from {tx.FromUsername} to {tx.ToUsername}"
                    });
                }
                
                _db.SaveChanges();


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

        public int CalculateRiskScore(User user, string currentIp, string currentLocation)
        {
            int score = 0;

            // 1. Location Mismatch (+50)
            if (!string.IsNullOrEmpty(user.HomeLocation) && 
                !LocationsAreSimilar(currentLocation, user.HomeLocation))
            {
                score += 50;
            }

            // 2. Multiple failed attempts (+30)
            // Define "multiple" as 3 or more total failed attempts in history
            if (user.FailedLoginCount >= 3)
            {
                score += 30;
            }

            // 3. Automatic logging if threshold met
            if (score >= 50)
            {
                _db.FraudLogs.Add(new FraudLog
                {
                    Description = $"High Risk Login Detected (Score: {score}) for {user.Username}. Location: {currentLocation}, IP: {currentIp}"
                });
                _db.SaveChanges();
            }

            return score;
        }
    }
}
