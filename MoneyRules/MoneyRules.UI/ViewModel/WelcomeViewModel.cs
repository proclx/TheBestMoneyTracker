using System.Windows.Input;
using System.Windows; // Для використання класу Window
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.Windows;
using MoneyRules.UI.Utils; // 👈 Використовуємо RelayCommand з папки Utils

namespace MoneyRules.UI.ViewModel
{
    public class WelcomeViewModel
    {
        // Зберігаємо всі залежності (Сервіси), щоб передати їх далі
        private readonly IAuthService _authService;
        private readonly ITransactionService _transactionService;
        private readonly IUserProfileService _profileService;
        private readonly IAdviceService _adviceService;
        private readonly ICurrencyService _currencyService;
        
        // Властивості ICommand для зв'язування з кнопками
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public WelcomeViewModel(
            IAuthService authService,
            ITransactionService transactionService,
            IUserProfileService profileService,
            IAdviceService adviceService,
            ICurrencyService currencyService)
        {
            _authService = authService;
            _transactionService = transactionService;
            _profileService = profileService;
            _adviceService = adviceService;
            _currencyService = currencyService;

            // Ініціалізація команд.
            // Примітка: використовуємо object? для сумісності з ICommand/.NET
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        // Логіка для кнопки "Увійти"
        private void ExecuteLogin(object? parameter)
        {
            // Перевіряємо, чи параметр є поточним вікном
            if (parameter is Window currentWindow)
            {
                var loginWindow = new LoginWindow(_authService);
                loginWindow.Show();
                currentWindow.Close();
            }
        }

        // Логіка для кнопки "Зареєструватися"
        private void ExecuteRegister(object? parameter)
        {
            // Перевіряємо, чи параметр є поточним вікном
            if (parameter is Window currentWindow)
            {
                var registerWindow = new RegisterWindow(
                    _authService, 
                    _transactionService, 
                    _profileService, 
                    _adviceService, 
                    _currencyService);
                    
                registerWindow.Show();
                currentWindow.Close();
            }
        }
    }
}