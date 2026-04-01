using Microsoft.Maui.Controls;
using SecureBankingApp.Services;
using SecureBankingApp.Models;
using SecureBankingApp.Database;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class TransactionHistoryPage : ContentPage
    {
        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        private readonly IServiceProvider _services;
        private readonly IAuthService _auth;
        private readonly ISessionService _session;

        public TransactionHistoryPage(IServiceProvider services, IAuthService auth, ISessionService session)
        {
            InitializeComponent();
            _services = services;
            _auth = auth;
            _session = session;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Session Protection
            if (!_session.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            string username = _auth.CurrentUsername!;
            
            using var scope = _services.CreateScope();
            var freshDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Transactions.Clear();
            var txs = freshDb.Transactions
                  .Where(t => t.FromUsername == username || t.ToUsername == username)
                  .OrderByDescending(t => t.Timestamp)
                  .ToList();
                  
            foreach(var tx in txs)
                Transactions.Add(tx);
        }
    }
}
