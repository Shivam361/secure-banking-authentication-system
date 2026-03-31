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
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var user = _db.Users.Single(u => u.Username == _auth.CurrentUsername);
            BalanceLabel.Text = $"Balance: {user.Balance:C}";
            // Fraud logs relevant to this user
            var userLogs = _db.FraudLogs.Where(f => f.Description.Contains(user.Username)).ToList();
            FraudBadge.Text = userLogs.Count.ToString();
            FraudBadge.IsVisible = userLogs.Count > 0;
            // Show admin controls based on role property
            AdminUserButton.IsVisible = user.IsAdmin;
            AdminTxButton.IsVisible = user.IsAdmin;
        }

        public MainPage(AppDbContext db, AuthService auth)
        {
            InitializeComponent();
            _db = db;
            _auth = auth;

            var username = auth.CurrentUsername!;
            var currentUser = db.Users.SingleOrDefault(u => u.Username == username);

            if (currentUser?.IsAdmin == true)
            {
                AdminUserButton.IsVisible = true;
                AdminTxButton.IsVisible = true;
            }

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

