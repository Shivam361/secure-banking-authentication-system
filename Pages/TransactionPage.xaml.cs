using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace SecureBankingApp.Pages
{
    public partial class TransactionPage : ContentPage, INotifyPropertyChanged
    {
        readonly FraudDetectionService _fraud;
        readonly AppDbContext _db;
        readonly string _username;

        bool _isBusy;
        public new bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(SubmitButtonText));
            }
        }
        public bool IsNotBusy => !IsBusy;
        public string SubmitButtonText => IsBusy ? "Processing…" : "Submit";

        public TransactionPage(AppDbContext db, FraudDetectionService fraud, AuthService auth)
        {
            InitializeComponent();
            BindingContext = this;
            _db = db;
            _fraud = fraud;
            _username = auth.CurrentUsername!;
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            TxErrorLabel.Text = "";
            TxSuccessLabel.Text = "";
            IsBusy = true;

            var toUser = ToUserEntry.Text?.Trim();
            var loc = LocationEntry.Text?.Trim();

            if (string.IsNullOrEmpty(toUser) || string.IsNullOrEmpty(AmountEntry.Text) || string.IsNullOrEmpty(loc))
            {
                TxErrorLabel.Text = "All fields are required.";
                IsBusy = false;
                return;
            }

            if (!_db.Users.Any(u => u.Username == toUser))
            {
                TxErrorLabel.Text = "Recipient user does not exist.";
                IsBusy = false;
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amt) || amt <= 0)
            {
                TxErrorLabel.Text = "Invalid amount.";
                IsBusy = false;
                return;
            }

            // Get sender and recipient users
            var fromUser = _db.Users.Single(u => u.Username == _username);
            var recipientUser = _db.Users.Single(u => u.Username == toUser);

            // Check sender balance
            if (fromUser.Balance < amt)
            {
                TxErrorLabel.Text = "Insufficient funds.";
                IsBusy = false;
                return;
            }

            await System.Threading.Tasks.Task.Delay(300);

            // Deduct and credit balances
            fromUser.Balance -= amt;
            recipientUser.Balance += amt;

            // Create and save transaction
            var tx = new Transaction
            {
                FromUsername = _username,   // sender (current user)
                ToUsername = toUser,        // recipient (entered in UI)
                Amount = amt,
                Location = loc
            };
            _db.Transactions.Add(tx);

            // Save all changes at once
            _db.SaveChanges();

            // Run fraud checks as before
            _fraud.ProcessTransaction(tx);
            if (amt >= 500)
            {
                await DisplayAlert("Warning", "This is a large transaction and has been flagged for review.", "OK");
            }

            IsBusy = false;
            TxSuccessLabel.Text = $"Sent {amt:C} to {toUser} at {loc}.";
            AmountEntry.Text = ToUserEntry.Text = LocationEntry.Text = "";
        }

    }
}
