namespace SecureBankingApp.Models
{
    /// <summary>
    /// Defines the available roles within the banking system.
    /// Stored as an integer in the database (EF Core default for enums).
    /// </summary>
    public enum UserRole
    {
        /// <summary>Standard bank customer — can view own data, make transactions.</summary>
        Customer = 0,

        /// <summary>Bank administrator — full access to all users, transactions, and fraud logs.</summary>
        Admin = 1
    }
}
