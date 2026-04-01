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
    public partial class AdminUserListPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly ISessionService _session;
        private readonly IRoleGuardService _guard;
        private readonly IAuditService _audit;
        public ObservableCollection<User> Users { get; set; }

        public AdminUserListPage(IServiceProvider services, ISessionService session, IRoleGuardService guard, IAuditService audit)
        {
            InitializeComponent();
            _services = services;
            _session = session;
            _guard = guard;
            _audit = audit;
            Users = new ObservableCollection<User>();
            UserList.ItemsSource = Users;
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

            _audit.LogAction(_session.CurrentUsername!, "Accessed User Management Console");

            // 3. Fetch fresh transactions
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Users.Clear();
            var freshUsers = db.Users.ToList();
            foreach (var u in freshUsers)
                Users.Add(u);
        }
    }
}
