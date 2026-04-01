using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace SecureBankingApp.Pages
{
    public partial class TransactionPage : ContentPage, INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly IAuthService _auth;
        private readonly IFraudDetectionService _fraud;
        private readonly ISessionService _session;
        private readonly string _username;
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(SubmitButtonText));
            }
        }

        public string SubmitButtonText => IsBusy ? "Processing..." : "Submit";

        public TransactionPage(IServiceProvider services, IAuthService auth, IFraudDetectionService fraud, ISessionService session)
        {
            InitializeComponent();
            _services = services;
            _auth = auth;
            _fraud = fraud;
            _session = session;
            _username = auth.CurrentUsername!;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_session.IsAuthenticated)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;

            IsBusy = true;
            TxErrorLabel.Text = "";
            TxSuccessLabel.Text = "";

            var toUser = ToUserEntry.Text?.Trim();
            var amtText = AmountEntry.Text?.Trim();
            var reference = ReferenceEntry.Text?.Trim();

            if (string.IsNullOrEmpty(toUser) || string.IsNullOrEmpty(amtText))
            {
                TxErrorLabel.Text = "Recipient and Amount are required.";
                IsBusy = false;
                return;
            }

            if (!decimal.TryParse(amtText, out decimal amt) || amt <= 0)
            {
                TxErrorLabel.Text = "Invalid amount.";
                IsBusy = false;
                return;
            }

            using var scope = _services.CreateScope();
            var freshDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!freshDb.Users.Any(u => u.Username == toUser))
            {
                TxErrorLabel.Text = "Recipient not found.";
                IsBusy = false;
                return;
            }

            // Get fresh sender and recipient users
            var fromUser = freshDb.Users.Single(u => u.Username == _username);
            var recipientUser = freshDb.Users.Single(u => u.Username == toUser);

            if (fromUser.Balance < amt)
            {
                TxErrorLabel.Text = "Insufficient funds.";
                IsBusy = false;
                return;
            }

            // Simulate network processing
            await System.Threading.Tasks.Task.Delay(800);

            // Deduct and credit balances
            fromUser.Balance -= amt;
            recipientUser.Balance += amt;

            // Secure physical location stealthily for fraud detection
            string? loc = null;
            try
            {
                var location = await Microsoft.Maui.Devices.Sensors.Geolocation.GetLastKnownLocationAsync();
                if (location != null) loc = $"{location.Latitude},{location.Longitude}";
            }
            catch { loc = "Unknown API Call"; }

            // Create and save transaction
            var tx = new Transaction
            {
                FromUsername = _username,
                ToUsername = toUser,
                Amount = amt,
                Reference = reference,
                Location = loc, // Silently appended
                Timestamp = DateTime.UtcNow
            };

            freshDb.Transactions.Add(tx);
            await freshDb.SaveChangesAsync();

            // Run fraud checks asynchronously
            _ = System.Threading.Tasks.Task.Run(() => {
                try {
                    using var fraudScope = _services.CreateScope();
                    var fraudDb = fraudScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var fraudService = fraudScope.ServiceProvider.GetRequiredService<IFraudDetectionService>();
                    
                    fraudService.ProcessTransaction(tx);
                } catch { } 
            });

            if (amt >= 500)
            {
                await DisplayAlert("Notice", "Large transactions may be subject to review.", "OK");
            }

            IsBusy = false;
            TxSuccessLabel.Text = $"Sent {amt:C} to {toUser}.";
            
            AmountEntry.Text = ToUserEntry.Text = ReferenceEntry.Text = "";

            await DisplayAlert("Success", $"You have successfully transferred {amt:C} to {toUser}.", "Return to Dashboard");
            await Navigation.PopAsync();
        }
    }
}
