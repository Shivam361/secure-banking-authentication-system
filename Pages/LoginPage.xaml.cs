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
            IsBusy = true;
            ErrorLabel.Text = "";

            var username = UsernameEntry.Text?.Trim() ?? "";
            var password = PasswordEntry.Text ?? "";

            // Get user (before password check, to check lockout)
            var user = _auth.GetUser(username); // <-- you may need to add this method, see below
            if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                ErrorLabel.Text = $"Account locked until {user.LockoutEnd.Value:t}.";
                IsBusy = false;
                return;
            }

            // Verify credentials
            var ok = _auth.VerifyPassword(username, password, out user);

            // Log login attempt with dummy IP for now (still useful for stats)
            _fraud.LogAttempt(username, ok, "127.0.0.1");

            if (!ok || user is null)
            {
                // Brute-force logic
                if (user != null)
                {
                    user.FailedLoginCount += 1;
                    if (user.FailedLoginCount >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(5); // lock for 5 min
                        _fraud.LogFraud(user.Username, "Brute-force detected", "N/A");
                    }
                    _auth.SaveUser(user); // <-- ensure this persists to DB
                }

                ErrorLabel.Text = "Invalid credentials.";
                IsBusy = false;
                return;
            }
            else
            {
                // Reset failed count on successful login
                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                _auth.SaveUser(user); // <-- ensure this persists to DB
            }

            // --- Check login location against home location ---
            string loginLocation = await NetworkHelper.GetCurrentLocationAsync();

            var normalizedHome = (user.HomeLocation ?? "").Trim().ToLowerInvariant();
            // Only log fraud if the locations are not similar
            if (!string.IsNullOrEmpty(normalizedHome) && !string.IsNullOrEmpty(normalizedLogin) &&
                !_fraud.LocationsAreSimilar(loginLocation, user.HomeLocation))
            {
                _fraud.LogFraud(user.Username, loginLocation ?? "Unknown", user.HomeLocation);
            }

            // --- End location check ---

            // Generate OTP and send via email
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

