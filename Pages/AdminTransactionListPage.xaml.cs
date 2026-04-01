using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SecureBankingApp.Pages
{
    public partial class AdminTransactionListPage : ContentPage
    {
        private readonly ISessionService _session;
        private readonly IServiceProvider _services;
        private readonly IRoleGuardService _guard;
        public ObservableCollection<Transaction> Transactions { get; set; }

        public AdminTransactionListPage(IServiceProvider services, ISessionService session, IRoleGuardService guard)
        {
            InitializeComponent();
            _guard = guard;
            _services = services;
            _session = session;
            Transactions = new ObservableCollection<Transaction>();
            TxList.ItemsSource = Transactions;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // 1. Session Guard
            if (!_session.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            // 2. Role Guard
            if (!await _guard.EnforceRoleAsync(this, UserRole.Admin))
                return;

            // 3. Fetch fresh transactions
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Transactions.Clear();
            var freshTxs = db.Transactions.OrderByDescending(t => t.Timestamp).ToList();
            foreach (var tx in freshTxs)
                Transactions.Add(tx);
        }
    }
}
