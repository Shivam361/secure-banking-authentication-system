// MauiProgram.cs
using System;
using System.IO;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // --- Register EF Core DbContext ---
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "banking.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // --- Register application services ---
            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<FraudDetectionService>();
            builder.Services.AddSingleton<RoleGuardService>();

            // --- Register pages for DI navigation ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<OtpPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RegistrationPage>();
            // in MauiProgram.CreateMauiApp(), after other pages:
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
            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var auth = scope.ServiceProvider.GetRequiredService<AuthService>();

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

            // Always return the built app
            return app;
        }
    }
}
