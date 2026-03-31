using SecureBankingApp.Database;
using SecureBankingApp.Models;

namespace SecureBankingApp.Services
{
    /// <summary>
    /// Centralised role-based access control service.
    /// Pages call this to verify the current user has the required role
    /// before displaying restricted content or performing privileged operations.
    /// </summary>
    public class RoleGuardService
    {
        private readonly AppDbContext _db;
        private readonly SessionService _session;

        public RoleGuardService(AppDbContext db, SessionService session)
        {
            _db = db;
            _session = session;
        }

        /// <summary>
        /// Returns the current authenticated user, or null if not logged in.
        /// </summary>
        public User? GetCurrentUser()
        {
            if (string.IsNullOrEmpty(_session.CurrentUsername))
                return null;

            return _db.Users.SingleOrDefault(u => u.Username == _session.CurrentUsername);
        }

        /// <summary>
        /// Checks whether the current user holds at least the specified role.
        /// </summary>
        public bool HasRole(UserRole requiredRole)
        {
            var user = GetCurrentUser();
            if (user == null) return false;

            return user.Role >= requiredRole;
        }

        /// <summary>
        /// Checks whether the current user is an Admin.
        /// </summary>
        public bool IsAdmin => HasRole(UserRole.Admin);

        /// <summary>
        /// Enforces a role requirement on a page. If the user doesn't meet the
        /// required role, they are shown an alert and navigated back.
        /// Returns true if access is granted, false if denied.
        /// </summary>
        public async Task<bool> EnforceRoleAsync(ContentPage page, UserRole requiredRole)
        {
            if (HasRole(requiredRole))
                return true;

            await page.DisplayAlert(
                "Access Denied",
                "You do not have permission to view this page.",
                "OK");

            // Navigate back to the previous page
            if (page.Navigation.NavigationStack.Count > 1)
                await page.Navigation.PopAsync();

            return false;
        }
    }
}
