namespace SecureBankingApp.Services
{
    /// <summary>
    /// Holds the current user's session state (who is logged in).
    /// Registered as Singleton because it must survive across page navigations
    /// and scoped service lifetimes. Contains NO database dependency.
    /// </summary>
    public class SessionService : ISessionService
    {
        /// <summary>The username of the currently authenticated user, or null if not logged in.</summary>
        public string? CurrentUsername { get; private set; }

        /// <summary>True when a user has been authenticated.</summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUsername);

        /// <summary>Records a successful login.</summary>
        public void SetCurrentUser(string username)
        {
            CurrentUsername = username;
        }

        /// <summary>Clears the session (logout).</summary>
        public void Clear()
        {
            CurrentUsername = null;
        }
    }
}
