using System.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI.Windows
{
    public partial class WelcomeWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;

        public WelcomeWindow(IAuthService authService, ITransactionService transactionService)
        {
            InitializeComponent();
            _authService = authService;
            _transactionService = transactionService;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(_authService, _transactionService);
            loginWindow.Show();
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_authService, _transactionService);
            registerWindow.Show();
            Close();
        }
    }
}

