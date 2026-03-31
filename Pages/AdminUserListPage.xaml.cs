using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class AdminUserListPage : ContentPage
    {
        private readonly IRoleGuardService _guard;
        public ObservableCollection<User> Users { get; set; }

        public AdminUserListPage(AppDbContext db, IRoleGuardService guard)
        {
            InitializeComponent();
            _guard = guard;
            Users = new ObservableCollection<User>(db.Users.ToList());
            UserList.ItemsSource = Users;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _guard.EnforceRoleAsync(this, UserRole.Admin);
        }
    }
}
