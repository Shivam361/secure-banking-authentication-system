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
        public ObservableCollection<Transaction> Transactions { get; set; }

        public TransactionHistoryPage(AppDbContext db, IAuthService auth)
        {
            InitializeComponent();
            string username = auth.CurrentUsername!;
            Transactions = new ObservableCollection<Transaction>(
                db.Transactions
                  .Where(t => t.FromUsername == username || t.ToUsername == username)
                  .OrderByDescending(t => t.Timestamp)
                  .ToList()
            );

            BindingContext = this;
        }
    }
}
