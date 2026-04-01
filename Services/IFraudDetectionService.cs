using SecureBankingApp.Models;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Detects and logs suspicious activity — login anomalies and transaction fraud.
    /// </summary>
    public interface IFraudDetectionService
    {
        /// <summary>Logs a login attempt and flags suspicious patterns.</summary>
        void LogAttempt(string username, bool success, string ip);

        /// <summary>Analyses a transaction for fraud indicators.</summary>
        void ProcessTransaction(Transaction tx);

        /// <summary>Checks whether two location strings are similar.</summary>
        bool LocationsAreSimilar(string? locA, string? locB);

        /// <summary>Calculates a risk score for a login attempt. Scores >= 50 flag automatic fraud logs.</summary>
        int CalculateRiskScore(User user, string currentIp, string currentLocation);

        /// <summary>Records a fraud event for a location mismatch.</summary>
        void LogFraud(string username, string? loginLocation, string? homeLocation);
    }
}
