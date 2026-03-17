using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using SecureBankingApp.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureBankingApp.Pages
{
    public partial class OtpPage : ContentPage, INotifyPropertyChanged
    {
        readonly AuthService _auth;
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
                OnPropertyChanged(nameof(VerifyButtonText));

                if (value) StartShimmer(); else StopShimmer();
            }
        }

        public bool IsNotBusy => !IsBusy;
        public string VerifyButtonText => IsBusy ? "Verifying…" : "Verify Code";

        public OtpPage(AuthService auth)
        {
            InitializeComponent();
            BindingContext = this;

            _auth = MauiProgram.ServiceProvider.GetRequiredService<AuthService>();
            _username = auth.CurrentUsername!;
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
            var dashboard = MauiProgram.ServiceProvider.GetRequiredService<MainPage>();
            await Navigation.PushAsync(
  MauiProgram.ServiceProvider.GetRequiredService<MainPage>()
);
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
