using System.Threading.Tasks;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Abstraction for sending OTP emails. Enables easy swapping/mocking of email providers.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send an OTP code to the given email address.
        /// Returns true if the email was sent successfully, false otherwise.
        /// </summary>
        Task<bool> SendOtpAsync(string toEmail, string otpCode);

        /// <summary>
        /// Indicates whether the email service is properly configured and ready to send.
        /// </summary>
        bool IsConfigured { get; }
    }
}
