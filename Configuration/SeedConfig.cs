namespace SecureBankingApp.Configuration
{
    /// <summary>
    /// Default admin seed settings.
    /// In production, these would be loaded from appsettings.json, environment
    /// variables, or a secrets manager — never hardcoded in source.
    /// </summary>
    public static class SeedConfig
    {
        // ── Pull from environment variables at startup, fall back to defaults ──
        public static string AdminUsername =>
            Environment.GetEnvironmentVariable("BANK_ADMIN_USER") ?? "admin";

        public static string AdminPassword =>
            Environment.GetEnvironmentVariable("BANK_ADMIN_PASS") ?? "Admin@123";

        public static string AdminEmail =>
            Environment.GetEnvironmentVariable("BANK_ADMIN_EMAIL") ?? "admin@bank.com";

        public static decimal AdminInitialBalance => 1_000_000m;
    }
}
