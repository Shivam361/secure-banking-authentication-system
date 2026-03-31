using SecureBankingApp.Models;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Handles user authentication, password hashing, and OTP management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>The username of the currently authenticated user.</summary>
        string? CurrentUsername { get; }

        /// <summary>Hashes a plaintext password using a secure algorithm (BCrypt).</summary>
        string HashPassword(string plain);

        /// <summary>Verifies credentials. Sets session on success.</summary>
        bool VerifyPassword(string username, string plain, out User? user);

        /// <summary>Generates, stores, and emails a 6-digit OTP.</summary>
        Task<(bool EmailSent, string OtpCode)> GenerateAndSendOtpAsync(string username, string email);

        /// <summary>Validates an OTP code and consumes it on success.</summary>
        bool ValidateOtp(string username, string code);

        /// <summary>Retrieves a user by username.</summary>
        User? GetUser(string username);

        /// <summary>Persists changes to a user entity.</summary>
        void SaveUser(User user);
    }
}
