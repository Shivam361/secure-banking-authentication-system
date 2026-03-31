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

        public MainPage(AppDbContext db, AuthService auth, RoleGuardService guard)
        {
            InitializeComponent();
            _db = db;
            _auth = auth;
            _guard = guard;

            // Show admin controls on initial load
            AdminUserButton.IsVisible = guard.IsAdmin;
            AdminTxButton.IsVisible = guard.IsAdmin;

            UsersCountLabel.Text = db.Users.Count().ToString();

        }
        async void OnNewTransactionClicked(object sender, EventArgs e)
        {
            var txPage = MauiProgram.ServiceProvider
                .GetRequiredService<TransactionPage>();
            await Navigation.PushAsync(
  MauiProgram.ServiceProvider.GetRequiredService<TransactionPage>()
);
        }
        async void OnViewTransactionsClicked(object sender, EventArgs e)
        {
            var page = MauiProgram.ServiceProvider.GetRequiredService<TransactionHistoryPage>();
            await Navigation.PushAsync(page);
        }
        async void OnFraudLogClicked(object sender, EventArgs e)
        {
            var page = MauiProgram.ServiceProvider.GetRequiredService<FraudLogPage>();
            await Navigation.PushAsync(page);
        }
        async void OnAdminUserClicked(object sender, EventArgs e)
        {
            var page = MauiProgram.ServiceProvider.GetRequiredService<AdminUserListPage>();
            await Navigation.PushAsync(page);
        }
        async void OnAdminTxClicked(object sender, EventArgs e)
        {
            var page = MauiProgram.ServiceProvider.GetRequiredService<AdminTransactionListPage>();
            await Navigation.PushAsync(page);
        }


    }
}

