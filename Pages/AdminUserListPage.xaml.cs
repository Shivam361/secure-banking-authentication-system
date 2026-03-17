using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class AdminUserListPage : ContentPage
    {
        public ObservableCollection<User> Users { get; set; }

        public AdminUserListPage(AppDbContext db)
        {
            InitializeComponent();
            Users = new ObservableCollection<User>(db.Users.ToList());
            UserList.ItemsSource = Users;
        }
    }
}
