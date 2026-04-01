namespace SecureBankingApp.Services
{
    /// <summary>
    /// Holds the current user's authentication session state.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>The full user object of the currently authenticated session, or null.</summary>
        SecureBankingApp.Models.User? CurrentUser { get; }

        /// <summary>The username of the currently authenticated user, or null.</summary>
        string? CurrentUsername { get; }

        /// <summary>True when a user has been authenticated.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Starts a new session for the given user.</summary>
        void Login(SecureBankingApp.Models.User user);

        /// <summary>Clears the session (logout).</summary>
        void Logout();
    }
}
