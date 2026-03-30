namespace SecureBankingApp.Services
{
    /// <summary>
    /// SMTP configuration for OTP email delivery.
    /// 
    /// For Gmail: enable 2FA on your Google account, then generate an App Password at
    /// https://myaccount.google.com/apppasswords — use that as the Password below.
    /// 
    /// ⚠️ DO NOT commit real credentials to source control.
    /// </summary>
    public static class EmailConfig
    {
        // ── SMTP Server Settings ──────────────────────────────────────────
        public static string SmtpHost { get; set; } = "smtp.gmail.com";
        public static int SmtpPort { get; set; } = 587;
        public static bool UseSsl { get; set; } = true; // STARTTLS on port 587

        // ── Credentials ───────────────────────────────────────────────────
        // Replace with your Gmail address and App Password
        public static string SenderEmail { get; set; } = "";
        public static string SenderPassword { get; set; } = "";
        public static string SenderDisplayName { get; set; } = "Secure Banking App";

        /// <summary>
        /// Returns true when valid SMTP credentials have been provided.
        /// When false, OTP delivery falls back to DisplayAlert.
        /// </summary>
        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(SenderEmail) &&
            !string.IsNullOrWhiteSpace(SenderPassword);
    }
}
