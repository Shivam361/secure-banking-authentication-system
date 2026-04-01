using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SecureBankingApp.Database;
using SecureBankingApp.Models;

namespace SecureBankingApp.Services
{
    public class SessionService : ISessionService
    {
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentJwtToken);

        public string? CurrentJwtToken { get; private set; }

        public void SetToken(string jwtToken)
        {
            CurrentJwtToken = jwtToken;
        }

        public User? GetCurrentUser(IServiceProvider services)
        {
            if (string.IsNullOrEmpty(CurrentJwtToken)) return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtTask = handler.ReadJwtToken(CurrentJwtToken);
                var usernameClaim = jwtTask.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

                if (string.IsNullOrEmpty(usernameClaim)) return null;

                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return db.Users.SingleOrDefault(u => u.Username == usernameClaim);
            }
            catch
            {
                return null;
            }
        }

        public void Logout()
        {
            CurrentJwtToken = null;
        }
    }
}
