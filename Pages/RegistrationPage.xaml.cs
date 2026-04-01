using Microsoft.Maui.Controls;
using SecureBankingApp.Database;
using SecureBankingApp.Services;
using SecureBankingApp.Models;
using SecureBankingApp.Helpers;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecureBankingApp.Pages
{
    public partial class RegistrationPage : ContentPage
    {
        private readonly AppDbContext _db;
        private readonly IAuthService _auth;

        public RegistrationPage(AppDbContext db, IAuthService auth)
        {
            InitializeComponent();
            _db = db;
            _auth = auth;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            RegErrorLabel.Text = "";
            RegSuccessLabel.Text = "";

            var username = UsernameEntry.Text?.Trim();
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;
            var confirm = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                RegErrorLabel.Text = "All fields are required.";
                return;
            }

            // --- New Validation logic ---
            if (!ValidationHelper.IsValidEmail(email))
            {
                RegErrorLabel.Text = "Please enter a valid email address.";
                return;
            }

            if (!ValidationHelper.IsValidPassword(password, out string passwordError))
            {
                RegErrorLabel.Text = passwordError;
                return;
            }

            if (_db.Users.Any(u => u.Username == username))
            {
                RegErrorLabel.Text = "Username already exists.";
                return;
            }

            if (password != confirm)
            {
                RegErrorLabel.Text = "Passwords do not match.";
                return;
            }
            string homeLocation = await NetworkHelper.GetCurrentLocationAsync();


            // Add user
            _db.Users.Add(new User
            {
                Username = username,
                Email = email,
                PasswordHash = _auth.HashPassword(password),
                Balance = 1000.0m, // Initial balance
                HomeLocation = homeLocation,
            });
            _db.SaveChanges();

            RegSuccessLabel.Text = "Registration successful! You can now login.";
            UsernameEntry.Text = EmailEntry.Text = PasswordEntry.Text = ConfirmPasswordEntry.Text = "";
        }
        async void OnLoginRedirectClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
