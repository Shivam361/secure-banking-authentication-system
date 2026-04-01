namespace SecureBankingApp.Services
{
    /// <summary>
    /// Holds the current user's session state (who is logged in).
    /// Registered as Singleton because it must survive across page navigations
    /// and scoped service lifetimes. Contains NO database dependency.
    /// </summary>
    public class SessionService : ISessionService
    {
        public SecureBankingApp.Models.User? CurrentUser { get; private set; }

        public string? CurrentUsername => CurrentUser?.Username;

        public bool IsAuthenticated => CurrentUser != null;

        public void Login(SecureBankingApp.Models.User user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
