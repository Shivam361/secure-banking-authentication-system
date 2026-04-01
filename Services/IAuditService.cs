namespace SecureBankingApp.Services
{
    public interface IAuditService
    {
        void LogAction(string username, string action);
    }
}
