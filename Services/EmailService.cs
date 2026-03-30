using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Sends OTP emails via SMTP using MailKit.
    /// Falls back gracefully when SMTP is not configured.
    /// </summary>
    public class EmailService : IEmailService
    {
        public bool IsConfigured => EmailConfig.IsConfigured;

        public async Task<bool> SendOtpAsync(string toEmail, string otpCode)
        {
            if (!IsConfigured)
            {
                Debug.WriteLine($"[EmailService] SMTP not configured. OTP for {toEmail}: {otpCode}");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(EmailConfig.SenderDisplayName, EmailConfig.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "Your Secure Banking OTP Code";

                // Build a clean HTML email body
                message.Body = new BodyBuilder
                {
                    HtmlBody = BuildOtpEmailHtml(otpCode),
                    TextBody = $"Your OTP verification code is: {otpCode}\n\nThis code expires in 2 minutes. Do not share it with anyone."
                }.ToMessageBody();

                using var client = new SmtpClient();

                // Connect with STARTTLS on port 587 (standard for Gmail)
                await client.ConnectAsync(
                    EmailConfig.SmtpHost,
                    EmailConfig.SmtpPort,
                    SecureSocketOptions.StartTls);

                // Authenticate with Gmail App Password
                await client.AuthenticateAsync(
                    EmailConfig.SenderEmail,
                    EmailConfig.SenderPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(quit: true);

                Debug.WriteLine($"[EmailService] OTP email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EmailService] Failed to send OTP email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Builds a professional HTML email template for the OTP code.
        /// </summary>
        private static string BuildOtpEmailHtml(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>
<body style=""margin:0; padding:0; font-family: 'Segoe UI', Arial, sans-serif; background-color:#f4f4f8;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <table width=""420"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff; border-radius:12px; box-shadow: 0 2px 12px rgba(0,0,0,0.08); overflow:hidden;"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #512BD4, #7B5FE0); padding: 28px 32px; text-align:center;"">
                            <h1 style=""margin:0; color:#ffffff; font-size:22px; font-weight:600; letter-spacing:0.5px;"">
                                🔐 Secure Banking
                            </h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding: 36px 32px 24px;"">
                            <p style=""margin:0 0 8px; color:#333; font-size:16px; font-weight:600;"">
                                Verification Code
                            </p>
                            <p style=""margin:0 0 24px; color:#666; font-size:14px; line-height:1.5;"">
                                Use the following code to complete your login. This code is valid for <strong>2 minutes</strong>.
                            </p>
                            <!-- OTP Code Box -->
                            <div style=""background:#f8f6ff; border: 2px dashed #512BD4; border-radius:8px; padding:20px; text-align:center; margin: 0 0 24px;"">
                                <span style=""font-size:36px; font-weight:700; letter-spacing:8px; color:#512BD4; font-family: 'Courier New', monospace;"">
                                    {otpCode}
                                </span>
                            </div>
                            <p style=""margin:0; color:#999; font-size:12px; line-height:1.5;"">
                                If you did not request this code, please ignore this email or contact support immediately.
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 16px 32px; background:#fafafa; border-top: 1px solid #eee; text-align:center;"">
                            <p style=""margin:0; color:#aaa; font-size:11px;"">
                                Secure Banking App &mdash; Never share your OTP with anyone.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
