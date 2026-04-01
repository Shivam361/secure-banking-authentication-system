namespace SecureBankingApp.Services
{
    /// <summary>
    /// Holds the current user's authentication session state.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>True if there is an active valid JWT Token.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Gets the stored JWT token.</summary>
        string? CurrentJwtToken { get; }

        /// <summary>Establishes an active stateless session with a JWT.</summary>
        void SetToken(string jwtToken);

        /// <summary>Retrieves the dynamically decoded user from the local DB based on the JWT claim.</summary>
        SecureBankingApp.Models.User? GetCurrentUser(System.IServiceProvider services);

        /// <summary>Destroys the current unencrypted JWT from local memory.</summary>
        void Logout();
    }
}
