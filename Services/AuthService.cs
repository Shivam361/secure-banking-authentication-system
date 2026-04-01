using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System.Security.Cryptography;

namespace SecureBankingApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ISessionService _session;
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        // BCrypt work factor — 2^11 iterations; increase to 12+ as hardware improves
        private const int WorkFactor = 11;

        public AuthService(AppDbContext db, IEmailService emailService, ISessionService session)
        {
            _db = db;
            _emailService = emailService;
            _session = session;
        }

        /// <summary>
        /// Hash a plaintext password using BCrypt with an auto-generated salt.
        /// Returns a 60-character BCrypt hash string (includes algorithm, cost, salt, and hash).
        /// </summary>
        public string HashPassword(string plain)
        {
            return BCrypt.Net.BCrypt.HashPassword(plain, workFactor: WorkFactor);
        }

        /// <summary>Read-through to SessionService for backward compatibility.</summary>
        public string? CurrentUsername => _session.CurrentUsername;

        /// <summary>
        /// Verify login credentials using BCrypt's timing-safe comparison.
        /// Implements account lockout logic (5 attempts, 15 minute lock).
        /// </summary>
        public bool VerifyPassword(string username, string plain, out User? user)
        {
            user = _db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
                return false;

            // 1. Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                // Account is still locked
                return false;
            }

            // 2. Perform verification
            bool ok = BCrypt.Net.BCrypt.Verify(plain, user.PasswordHash);

            if (ok)
            {
                // Success: Reset failures and lockout
                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                _db.Users.Update(user);
                _db.SaveChanges();

                // Record in session
                _session.Login(user);
            }
            else
            {
                // Failure: Increment count
                user.FailedLoginCount++;
                
                if (user.FailedLoginCount >= 5)
                {
                    // Lock for 15 minutes
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                }

                _db.Users.Update(user);
                _db.SaveChanges();
            }

            return ok;
        }

        /// <summary>
        /// Generate a 6-digit OTP, store it in the database, and attempt to send it via email.
        /// Returns (emailSent: true if email was delivered, otpCode: the generated code for fallback display).
        /// </summary>
        public async Task<(bool EmailSent, string OtpCode)> GenerateAndSendOtpAsync(string username, string email)
        {
            var now = DateTime.UtcNow;

            // Remove old OTPs for this user
            var oldOtps = _db.OTPRequests.Where(r => r.Username == username);
            _db.OTPRequests.RemoveRange(oldOtps);

            var bytes = new byte[4];
            _rng.GetBytes(bytes);
            var code = (BitConverter.ToUInt32(bytes, 0) % 900000 + 100000).ToString();

            var req = new OTPRequest
            {
                Username = username,
                Code = code,
                ExpiresAt = now.AddMinutes(2)
            };

            _db.OTPRequests.Add(req);
            _db.SaveChanges();

            // Attempt email delivery
            var sent = await _emailService.SendOtpAsync(email, code);

            return (sent, code);
        }

        // Validate OTP and consume it
        // Implements rate limiting (5 validation attempts)
        public bool ValidateOtp(string username, string code)
        {
            var now = DateTime.UtcNow;
            var req = _db.OTPRequests.SingleOrDefault(r => r.Username == username && r.ExpiresAt >= now);

            if (req == null) return false;

            if (req.ValidationAttempts >= 5)
            {
                // Brute force detected — consume and fail
                _db.OTPRequests.Remove(req);
                _db.SaveChanges();
                return false;
            }

            if (req.Code != code)
            {
                req.ValidationAttempts++;
                _db.OTPRequests.Update(req);
                _db.SaveChanges();
                return false;
            }

            // Success: consume and allow
            _db.OTPRequests.Remove(req);
            
            // Cleanup other expired OTPs while we are here
            var expired = _db.OTPRequests.Where(r => r.ExpiresAt < now);
            _db.OTPRequests.RemoveRange(expired);
            
            _db.SaveChanges();
            return true;
        }
        public User? GetUser(string username)
        {
            return _db.Users.SingleOrDefault(u => u.Username == username);
        }

        public void SaveUser(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }

    }
}

