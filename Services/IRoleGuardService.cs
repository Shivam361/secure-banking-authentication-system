using SecureBankingApp.Models;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Centralised role-based access control — checks and enforces user permissions.
    /// </summary>
    public interface IRoleGuardService
    {
        /// <summary>Returns the current authenticated user, or null.</summary>
        User? GetCurrentUser();

        /// <summary>Checks whether the current user holds at least the specified role.</summary>
        bool HasRole(UserRole requiredRole);

        /// <summary>True when the current user is an Admin.</summary>
        bool IsAdmin { get; }

        /// <summary>Enforces a role on a page — shows "Access Denied" and navigates back if denied.</summary>
        Task<bool> EnforceRoleAsync(ContentPage page, UserRole requiredRole);
    }
}
