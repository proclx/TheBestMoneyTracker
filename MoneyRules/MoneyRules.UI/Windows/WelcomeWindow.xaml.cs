using System.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI.Windows
{
    public partial class WelcomeWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService;
        private readonly IAdviceService _adviceService;
        private readonly ICurrencyService _currencyService;

        public WelcomeWindow(
            IAuthService authService,
            ITransactionService transactionService,
            IUserProfileService profileService,
            IAdviceService adviceService,
            ICurrencyService currencyService)
        {
            InitializeComponent();
            _authService = authService;
            _transactionService = transactionService;
            _profileService = profileService;
            _adviceService = adviceService;
            _currencyService = currencyService;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(_authService, _transactionService, _profileService, _adviceService, _currencyService);
            loginWindow.Show();
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_authService, _transactionService, _profileService, _adviceService, _currencyService);
            registerWindow.Show();
            Close();
        }
    }
}

