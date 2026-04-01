using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecureBankingApp.Pages
{
    public partial class LoginPage : ContentPage, INotifyPropertyChanged
    {
        readonly IAuthService _auth;
        readonly IFraudDetectionService _fraud;
        readonly IServiceProvider _services;

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
                OnPropertyChanged(nameof(LoginButtonText));
            }
        }

        public bool IsNotBusy => !IsBusy;
        public string LoginButtonText => IsBusy ? "Processing…" : "Login";

        public LoginPage(IAuthService auth, IFraudDetectionService fraud, IServiceProvider services)
        {
            InitializeComponent();
            BindingContext = this;

            _auth = auth;
            _fraud = fraud;
            _services = services;
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var regPage = _services.GetRequiredService<RegistrationPage>();
            await Navigation.PushAsync(regPage);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorLabel.Text = "";

            var username = UsernameEntry.Text?.Trim() ?? "";
            var password = PasswordEntry.Text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorLabel.Text = "Please enter both credentials.";
                IsBusy = false;
                return;
            }

            // 1. Verify Credentials (AuthService handles Session and DB state internally now)
            var ok = _auth.VerifyPassword(username, password, out var user);

            // Fetch IP and Location for fraud/risk analysis
            string currentIp = "127.0.0.1"; // Default fallback
            string currentLoc = await NetworkHelper.GetCurrentLocationAsync();

            if (user != null)
            {
                // 2. Perform Risk Analysis
                _fraud.CalculateRiskScore(user, currentIp, currentLoc);

                // 3. Handle Lockout Response
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    ErrorLabel.Text = $"Account locked. Try again after {user.LockoutEnd.Value:t}.";
                    IsBusy = false;
                    return;
                }
            }

            // Log raw attempt for auditing
            _fraud.LogAttempt(username, ok, currentIp);

            if (!ok || user == null)
            {
                ErrorLabel.Text = "Invalid username or password.";
                IsBusy = false;
                return;
            }

            // 4. Generate OTP and send via email
            var (emailSent, otpCode) = await _auth.GenerateAndSendOtpAsync(username, user.Email);

            await Task.Delay(250);

            if (emailSent)
            {
                // Email delivered — show confirmation without revealing the code
                var maskedEmail = MaskEmail(user.Email);
                await DisplayAlert("OTP Sent",
                    $"A verification code has been sent to {maskedEmail}.\nIt expires in 2 minutes.",
                    "OK");
            }
            else
            {
                // SMTP not configured — fallback to DisplayAlert for development convenience
                await DisplayAlert("OTP (Dev Mode)",
                    $"Email delivery unavailable.\nYour code is: {otpCode}",
                    "OK");
            }

            IsBusy = false;

            // Navigate to OTP page
            await Navigation.PushAsync(
                _services.GetRequiredService<OtpPage>()
            );
        }
        /// <summary>
        /// Masks an email address for display: "shivam@gmail.com" → "sh***@gmail.com"
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return "your registered email";

            var parts = email.Split('@');
            var local = parts[0];
            var domain = parts[1];

            var visible = local.Length >= 2 ? local[..2] : local[..1];
            return $"{visible}***@{domain}";
        }

    }
}

