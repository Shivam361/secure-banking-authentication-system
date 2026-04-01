using Microsoft.Maui.Controls;

namespace SecureBankingApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
