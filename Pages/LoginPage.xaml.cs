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
        readonly ISessionService _session;

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

        public LoginPage(IAuthService auth, IFraudDetectionService fraud, IServiceProvider services, ISessionService session)
        {
            InitializeComponent();
            BindingContext = this;

            _auth = auth;
            _fraud = fraud;
            _services = services;
            _session = session;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            var lastUsername = Microsoft.Maui.Storage.Preferences.Get("LastUsername", string.Empty);
            if (!string.IsNullOrEmpty(lastUsername))
            {
                UsernameEntry.Text = lastUsername;
                
                try 
                {
                    var user = _auth.GetUser(lastUsername);
                    if (user != null && user.BiometricEnabled)
                    {
                        var isAvailable = await Plugin.Fingerprint.CrossFingerprint.Current.IsAvailableAsync(true);
                        if (isAvailable)
                        {
                            BiometricButton.IsVisible = true;
                        }
                    }
                } 
                catch { } // Handle missing plugin/device errors gracefully
            }
        }

        private async void OnBiometricClicked(object sender, EventArgs e)
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorLabel.Text = "";

            var username = UsernameEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(username)) 
            {
                ErrorLabel.Text = "Username required for Biometrics.";
                IsBusy = false;
                return;
            }

            var request = new Plugin.Fingerprint.Abstractions.AuthenticationRequestConfiguration
                ("Secure Login", "Verify your identity to access Secure Bank.");

            var result = await Plugin.Fingerprint.CrossFingerprint.Current.AuthenticateAsync(request);
            if (result.Authenticated)
            {
                var user = _auth.GetUser(username);
                if (user != null) 
                {
                    // Generate JWT & Bypass OTP
                    var jwtMethod = _auth.GetType().GetMethod("GenerateJwtToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (jwtMethod != null)
                    {
                         var jwt = (string)jwtMethod.Invoke(_auth, new object[] { user })!;
                        _session.SetToken(jwt);
                        Microsoft.Maui.Storage.Preferences.Set("LastUsername", username);
                        IsBusy = false;

                        await Navigation.PushAsync(_services.GetRequiredService<MainPage>());
                        return;
                    }
                }
            }

            ErrorLabel.Text = "Biometric authentication failed or cancelled.";
            IsBusy = false;
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

            // Save for future biometric loads
            Microsoft.Maui.Storage.Preferences.Set("LastUsername", username);

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

