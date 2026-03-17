using Microsoft.Maui.Controls;
using SecureBankingApp.Pages;

namespace SecureBankingApp
{
    public partial class App : Application
    {
        public App(LoginPage loginPage)
        {
            InitializeComponent();
            MainPage = new NavigationPage(loginPage);
        }
    }
}
