using Microsoft.Maui.Controls;
using SecureBankingApp.Models;
using SecureBankingApp.Database;
using SecureBankingApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class FraudLogPage : ContentPage
    {
        public ObservableCollection<FraudLog> Logs { get; set; }
        public FraudLogPage(AppDbContext db, AuthService auth)
        {
            InitializeComponent();
            var user = db.Users.Single(u => u.Username == auth.CurrentUsername);
            // For admin, show all logs; for user, filter their username:
            if (user.Username == "admin") // or use a role property for real admin!
                Logs = new ObservableCollection<FraudLog>(db.FraudLogs.OrderByDescending(f => f.Timestamp).ToList());
            else
                Logs = new ObservableCollection<FraudLog>(db.FraudLogs
                    .Where(f => f.Description.Contains(user.Username))
                    .OrderByDescending(f => f.Timestamp)
                    .ToList());

            FraudList.ItemsSource = Logs;
        }
    }
}
