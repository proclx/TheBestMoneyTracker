using System.Windows;
using MoneyRules.Application.Services;

namespace MoneyRules.UI.Windows
{
    public partial class WelcomeWindow : Window
    {
        private readonly IAuthService _authService;

        public WelcomeWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(_authService);
            loginWindow.Show();
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }
    }
}
