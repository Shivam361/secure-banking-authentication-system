namespace SecureBankingApp.Services
{
    /// <summary>
    /// Holds the current user's authentication session state.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>The username of the currently authenticated user, or null.</summary>
        string? CurrentUsername { get; }

        /// <summary>True when a user has been authenticated.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Records a successful login.</summary>
        void SetCurrentUser(string username);

        /// <summary>Clears the session (logout).</summary>
        void Clear();
    }
}
