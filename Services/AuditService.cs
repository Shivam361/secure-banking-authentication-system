using Microsoft.Extensions.DependencyInjection;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System;

namespace SecureBankingApp.Services
{
    public class AuditService : IAuditService
    {
        private readonly IServiceProvider _services;

        public AuditService(IServiceProvider services)
        {
            _services = services;
        }

        public void LogAction(string username, string action)
        {
            try
            {
                // Run on a background thread so we don't block the UI
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        using var scope = _services.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        db.AuditLogs.Add(new AuditLog
                        {
                            Username = username,
                            Action = action,
                            Timestamp = DateTime.UtcNow
                        });

                        db.SaveChanges();
                    }
                    catch { } // Fail silently in audit log for background thread
                });
            }
            catch { }
        }
    }
}
