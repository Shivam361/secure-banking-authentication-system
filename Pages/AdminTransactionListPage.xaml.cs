using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class AdminTransactionListPage : ContentPage
    {
        public ObservableCollection<Transaction> Transactions { get; set; }

        public AdminTransactionListPage(AppDbContext db)
        {
            InitializeComponent();
            Transactions = new ObservableCollection<Transaction>(db.Transactions.OrderByDescending(t => t.Timestamp).ToList());
            TxList.ItemsSource = Transactions;
        }
    }
}
