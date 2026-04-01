using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class MainPage : ContentPage
    {

        readonly AppDbContext _db;
        readonly IAuthService _auth;
        readonly IRoleGuardService _guard;
        readonly IServiceProvider _services;
        readonly ISessionService _session;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // 1. Session Protection Guard
            if (!_session.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            var user = _session.CurrentUser;
            if (user == null) return;

            // Update UI with Session Data
            UserDisplayNameLabel.Text = user.Username;
            UserInitialsLabel.Text = user.Username.Length >= 2 ? user.Username[..2].ToUpper() : user.Username[..1].ToUpper();

            // Fetch a completely fresh DbContext scope to avoid EF Core stale cache during PopAsync back-navigation
            using var scope = _services.CreateScope();
            var freshDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dbUser = freshDb.Users.SingleOrDefault(u => u.Id == user.Id);
            if (dbUser == null) return;

            BalanceLabel.Text = $"{dbUser.Balance:C}"; // Display dynamic currency

            // Fraud logs relevant to this user
            var userLogs = freshDb.FraudLogs.Where(f => f.Description.Contains(dbUser.Username)).ToList();
            FraudBadge.Text = userLogs.Count.ToString();
            FraudBadgeFrame.IsVisible = userLogs.Count > 0;
            
            // Show admin controls based on role
            AdminUserButton.IsVisible = _guard.IsAdmin;
            AdminTxButton.IsVisible = _guard.IsAdmin;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _session.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        async void OnTopUpClicked(object sender, EventArgs e)
        {
            if (!_session.IsAuthenticated) return;

            using var scope = _services.CreateScope();
            var freshDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = freshDb.Users.SingleOrDefault(u => u.Id == _session.CurrentUser!.Id);
            
            if (user != null)
            {
                user.Balance += 500;
                
                var tx = new Transaction
                {
                    FromUsername = "SECURE_SYSTEM_DEPOSIT",
                    ToUsername = user.Username,
                    Amount = 500,
                    Reference = "Developer Top-Up",
                    Timestamp = DateTime.UtcNow
                };
                
                freshDb.Transactions.Add(tx);
                await freshDb.SaveChangesAsync();
                
                // Immediately update local UI
                BalanceLabel.Text = $"{user.Balance:C}";
                await DisplayAlert("Top-Up Successful", "Successfully deposited $500 into your account.", "OK");
            }
        }

        public MainPage(AppDbContext db, IAuthService auth, IRoleGuardService guard, IServiceProvider services, ISessionService session)
        {
            InitializeComponent();
            _db = db;
            _auth = auth;
            _guard = guard;
            _services = services;
            _session = session;

            // Show admin controls on initial load
            AdminUserButton.IsVisible = guard.IsAdmin;
            AdminTxButton.IsVisible = guard.IsAdmin;

            UsersCountLabel.Text = db.Users.Count().ToString();
        }

        async void OnNewTransactionClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<TransactionPage>();
            await Navigation.PushAsync(page);
        }

        async void OnViewTransactionsClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<TransactionHistoryPage>();
            await Navigation.PushAsync(page);
        }

        async void OnFraudLogClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<FraudLogPage>();
            await Navigation.PushAsync(page);
        }

        async void OnAdminUserClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<AdminUserListPage>();
            await Navigation.PushAsync(page);
        }

        async void OnAdminTxClicked(object sender, EventArgs e)
        {
            var page = _services.GetRequiredService<AdminTransactionListPage>();
            await Navigation.PushAsync(page);
        }
    }
}
