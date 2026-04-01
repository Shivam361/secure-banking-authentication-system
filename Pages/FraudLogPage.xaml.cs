using Microsoft.Maui.Controls;
using SecureBankingApp.Models;
using SecureBankingApp.Database;
using SecureBankingApp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace SecureBankingApp.Pages
{
    public partial class FraudLogPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly ISessionService _session;
        private readonly IRoleGuardService _guard;
        public ObservableCollection<FraudLog> Logs { get; set; }

        public FraudLogPage(IServiceProvider services, ISessionService session, IRoleGuardService guard)
        {
            InitializeComponent();
            _services = services;
            _session = session;
            _guard = guard;
            Logs = new ObservableCollection<FraudLog>();
            FraudList.ItemsSource = Logs;
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

            // 2. Fetch fresh logs
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var user = _session.CurrentUser!;
            
            Logs.Clear();
            List<FraudLog> freshLogs;

            if (_guard.IsAdmin)
                freshLogs = db.FraudLogs.OrderByDescending(f => f.Timestamp).ToList();
            else
                freshLogs = db.FraudLogs
                    .Where(f => f.Description.Contains(user.Username))
                    .OrderByDescending(f => f.Timestamp)
                    .ToList();

            foreach (var log in freshLogs)
                Logs.Add(log);
        }
    }
}
