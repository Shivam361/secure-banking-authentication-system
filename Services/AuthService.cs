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
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        // Hash a plaintext password
        public string HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes);
        }
        public string? CurrentUsername { get; private set; }

        // Verify login credentials
        public bool VerifyPassword(string username, string plain, out User? user)
        {
            user = _db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
                return false;

            // compute whether the hash matches
            bool ok = user.PasswordHash == HashPassword(plain);

            // if valid, remember who just logged in
            if (ok)
                CurrentUsername = username;

            return ok;
        }

        // Generate and store a 6-digit OTP valid for 2 minutes
        public string GenerateOtp(string username)
{
    var now = DateTime.UtcNow;

    //  Remove old OTPs for this user
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

    return code;
}

        // Validate OTP and consume it
        public bool ValidateOtp(string username, string code)
        {
            var now = DateTime.UtcNow;
            var req = _db.OTPRequests
                .Where(r => r.Username == username && r.Code == code && r.ExpiresAt >= now)
                .OrderByDescending(r => r.ExpiresAt)
                .FirstOrDefault();

                // 🔥 Cleanup expired OTPs
var expired = _db.OTPRequests.Where(r => r.ExpiresAt < now);
_db.OTPRequests.RemoveRange(expired);
_db.SaveChanges();

            if (req == null) return false;

            _db.OTPRequests.Remove(req);
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

