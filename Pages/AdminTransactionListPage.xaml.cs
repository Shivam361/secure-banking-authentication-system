using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class AdminTransactionListPage : ContentPage
    {
        private readonly IRoleGuardService _guard;
        public ObservableCollection<Transaction> Transactions { get; set; }

        public AdminTransactionListPage(AppDbContext db, IRoleGuardService guard)
        {
            InitializeComponent();
            _guard = guard;
            Transactions = new ObservableCollection<Transaction>(db.Transactions.OrderByDescending(t => t.Timestamp).ToList());
            TxList.ItemsSource = Transactions;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _guard.EnforceRoleAsync(this, UserRole.Admin);
        }
    }
}
