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
        readonly AuthService _auth;
        readonly RoleGuardService _guard;
        readonly IServiceProvider _services;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var user = _db.Users.Single(u => u.Username == _auth.CurrentUsername);
            BalanceLabel.Text = $"Balance: {user.Balance:C}";
            // Fraud logs relevant to this user
            var userLogs = _db.FraudLogs.Where(f => f.Description.Contains(user.Username)).ToList();
            FraudBadge.Text = userLogs.Count.ToString();
            FraudBadge.IsVisible = userLogs.Count > 0;
            // Show admin controls based on role
            AdminUserButton.IsVisible = _guard.IsAdmin;
            AdminTxButton.IsVisible = _guard.IsAdmin;
        }

        public MainPage(AppDbContext db, AuthService auth, RoleGuardService guard, IServiceProvider services)
        {
            InitializeComponent();
            _db = db;
            _auth = auth;
            _guard = guard;
            _services = services;

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
