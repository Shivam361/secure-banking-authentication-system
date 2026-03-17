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
        readonly AuthService _auth;
        readonly FraudDetectionService _fraud;

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

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = this;

            // resolve services from the DI container
            _auth = MauiProgram.ServiceProvider.GetRequiredService<AuthService>();
            _fraud = MauiProgram.ServiceProvider.GetRequiredService<FraudDetectionService>();
        }

        // --- Network helpers ---
        private async Task<string> GetPublicIpAsync()
        {
            using var http = new HttpClient();
            return await http.GetStringAsync("https://api.ipify.org");
        }
        private async Task<string> GetGeoLocationAsync(string ip)
        {
            using var http = new HttpClient();
            var json = await http.GetStringAsync($"http://ip-api.com/json/{ip}");
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var city = doc.RootElement.GetProperty("city").GetString();
            var country = doc.RootElement.GetProperty("country").GetString();
            return $"{city}, {country}";
        }
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            var regPage = MauiProgram.ServiceProvider.GetRequiredService<RegistrationPage>();
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
            string loginLocation = "Unknown";
            try
            {
                var ip = await GetPublicIpAsync();
                loginLocation = await GetGeoLocationAsync(ip);
            }
            catch
            {
                loginLocation = "Unknown";
            }

            var normalizedHome = (user.HomeLocation ?? "").Trim().ToLowerInvariant();
            var normalizedLogin = (loginLocation ?? "").Trim().ToLowerInvariant();

           // await DisplayAlert("Debug", $"Home: '{normalizedHome}'\nLogin: '{normalizedLogin}'", "OK");

            // Only log fraud if the locations are not similar
            if (!string.IsNullOrEmpty(normalizedHome) && !string.IsNullOrEmpty(normalizedLogin) &&
                !_fraud.LocationsAreSimilar(loginLocation, user.HomeLocation))
            {
                _fraud.LogFraud(user.Username, loginLocation ?? "Unknown", user.HomeLocation);
            }

            // --- End location check ---

            // Generate OTP (this happens for ALL successful logins)
            var code = _auth.GenerateOtp(username);

            await Task.Delay(250);
            await DisplayAlert("OTP", $"Your code is: {code}", "OK");
            IsBusy = false;

            // Navigate to OTP page
            await Navigation.PushAsync(
                MauiProgram.ServiceProvider.GetRequiredService<OtpPage>()
            );
        }

    }
}

