// MauiProgram.cs
using System;
using System.IO;
using CommunityToolkit.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using SecureBankingApp.Database;
using SecureBankingApp.Pages;
using SecureBankingApp.Services;
using SecureBankingApp.Models;

namespace SecureBankingApp
{
    public static class MauiProgram
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // --- App setup ---
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // --- Register EF Core DbContext ---
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "banking.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"), ServiceLifetime.Transient);

            // --- Register application services ---
            // Singleton: no database dependency — safe to live for the app's lifetime
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

            // Transient: these depend on AppDbContext (also Transient) — lifetimes must match
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IFraudDetectionService, FraudDetectionService>();
            builder.Services.AddTransient<IRoleGuardService, RoleGuardService>();

            // --- Register pages for DI navigation ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<OtpPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<TransactionPage>();
            builder.Services.AddTransient<TransactionHistoryPage>();
            builder.Services.AddTransient<FraudLogPage>();
            builder.Services.AddTransient<AdminUserListPage>();
            builder.Services.AddTransient<AdminTransactionListPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- Build and capture ServiceProvider ---
            var app = builder.Build();
            ServiceProvider = app.Services;

            // ─── SEED DEFAULT ADMIN (credentials from env vars / SeedConfig) ─────────────────────
            try 
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    // Ensure database is created before querying
                    db.Database.EnsureCreated();

                    try 
                    {
                        if (!db.Users.Any(u => u.Role == UserRole.Admin))
                        {
                            db.Users.Add(new SecureBankingApp.Models.User
                            {
                                Username     = Configuration.SeedConfig.AdminUsername,
                                PasswordHash = auth.HashPassword(Configuration.SeedConfig.AdminPassword),
                                Email        = Configuration.SeedConfig.AdminEmail,
                                Balance      = Configuration.SeedConfig.AdminInitialBalance,
                                Role         = UserRole.Admin,
                                LastLoginIP  = null
                            });
                            db.SaveChanges();
                        }
                    }
                    catch (Exception innerEx) when (innerEx.Message.Contains("no such column") || innerEx.InnerException?.Message.Contains("no such column") == true)
                    {
                        // Safely handles the migration reset ONLY if the schema is severely outdated on the local machine
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();

                        db.Users.Add(new SecureBankingApp.Models.User
                        {
                            Username     = Configuration.SeedConfig.AdminUsername,
                            PasswordHash = auth.HashPassword(Configuration.SeedConfig.AdminPassword),
                            Email        = Configuration.SeedConfig.AdminEmail,
                            Balance      = Configuration.SeedConfig.AdminInitialBalance,
                            Role         = UserRole.Admin,
                            LastLoginIP  = null
                        });
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Simple logging to a file in the app directory for troubleshooting
                var logFile = Path.Combine(Path.GetTempPath(), "maui_crash_program.txt");
                File.WriteAllText(logFile, ex.ToString());
            }

            // Always return the built app
            return app;
        }
    }
}
