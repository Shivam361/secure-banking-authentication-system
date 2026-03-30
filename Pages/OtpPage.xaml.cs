using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using SecureBankingApp.Database;
using SecureBankingApp.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SecureBankingApp.Pages
{
    public partial class OtpPage : ContentPage, INotifyPropertyChanged
    {
        readonly AuthService _auth;
        readonly string _username;
        readonly string _userEmail;

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
                OnPropertyChanged(nameof(VerifyButtonText));

                if (value) StartShimmer(); else StopShimmer();
            }
        }

        bool _canResend = true;
        public bool CanResend
        {
            get => _canResend;
            set
            {
                if (_canResend == value) return;
                _canResend = value;
                OnPropertyChanged(nameof(CanResend));
            }
        }

        public bool IsNotBusy => !IsBusy;
        public string VerifyButtonText => IsBusy ? "Verifying…" : "Verify Code";

        public OtpPage(AuthService auth, AppDbContext db)
        {
            InitializeComponent();
            BindingContext = this;

            _auth = MauiProgram.ServiceProvider.GetRequiredService<AuthService>();
            _username = auth.CurrentUsername!;

            // Look up the user's email for resend
            var user = db.Users.SingleOrDefault(u => u.Username == _username);
            _userEmail = user?.Email ?? "";
        }

        async void OnVerifyClicked(object sender, EventArgs e)
        {
            // 1) Clear any previous error state
            VisualStateManager.GoToState(OtpFrame, "Normal");
            OtpErrorLabel.Text = "";

            // 2) Show busy/shimmer
            IsBusy = true;

            await Task.Delay(200); // let shimmer show

            // 3) Validate OTP
            var ok = _auth.ValidateOtp(_username, OtpEntry.Text?.Trim() ?? "");

            // 4) Stop busy
            IsBusy = false;

            if (!ok)
            {
                // 5) Highlight error
                VisualStateManager.GoToState(OtpFrame, "Error");
                OtpErrorLabel.Text = "Invalid or expired code.";
                return;
            }

            // 6) Success → navigate
            await Navigation.PushAsync(
                MauiProgram.ServiceProvider.GetRequiredService<MainPage>()
            );
        }

        async void OnResendClicked(object sender, EventArgs e)
        {
            if (!CanResend || string.IsNullOrEmpty(_userEmail)) return;

            CanResend = false;
            OtpErrorLabel.Text = "";
            ResendCooldownLabel.TextColor = Colors.Gray;

            // Generate and send a new OTP
            var (emailSent, otpCode) = await _auth.GenerateAndSendOtpAsync(_username, _userEmail);

            if (emailSent)
            {
                ResendCooldownLabel.TextColor = Color.FromArgb("#27AE60");
                ResendCooldownLabel.Text = "✓ New code sent to your email.";
            }
            else
            {
                // Fallback: show OTP in alert for development convenience
                await DisplayAlert("OTP (Dev Mode)",
                    $"Email delivery unavailable.\nYour new code is: {otpCode}",
                    "OK");
                ResendCooldownLabel.Text = "Code displayed (dev mode).";
            }

            // Start 30-second cooldown
            await StartResendCooldownAsync();
        }

        /// <summary>
        /// 30-second cooldown timer before allowing another resend.
        /// </summary>
        private async Task StartResendCooldownAsync()
        {
            for (int remaining = 30; remaining > 0; remaining--)
            {
                ResendCooldownLabel.Text = $"Resend available in {remaining}s";
                ResendCooldownLabel.TextColor = Colors.Gray;
                await Task.Delay(1000);
            }

            ResendCooldownLabel.Text = "";
            CanResend = true;
        }


        #region shimmer helpers
        void StartShimmer()
        {
            Shimmer.IsVisible = true;
            Shimmer.TranslationX = -300;

            var animation = new Animation(v => Shimmer.TranslationX = v, -300, 300);
            animation.Commit(this, "shimmer", length: 1200, repeat: () => IsBusy);
        }

        void StopShimmer()
        {
            Shimmer.AbortAnimation("shimmer");
            Shimmer.IsVisible = false;
        }
        #endregion
    }
}
