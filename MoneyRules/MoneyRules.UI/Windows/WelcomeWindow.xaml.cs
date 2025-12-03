using System.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.ViewModel; // Додано

namespace MoneyRules.UI.Windows
{
    public partial class WelcomeWindow : Window
    {
        // Прибираємо поля лише якщо вони були потрібні тільки в обробниках Click
        // Залишаємо поля, щоб передати їх у конструктор ViewModel
        // private readonly IAuthService _authService; // ВИДАЛЕНІ, якщо вони не використовуються за межами конструктора
        // ... та інші поля

        public WelcomeWindow(
            IAuthService authService,
            ITransactionService transactionService,
            IUserProfileService profileService,
            IAdviceService adviceService,
            ICurrencyService currencyService)
        {
            InitializeComponent();
            
            // Встановлення DataContext з новим ViewModel
            this.DataContext = new WelcomeViewModel(
                authService,
                transactionService,
                profileService,
                adviceService,
                currencyService);
        }

        // *** ЦІ МЕТОДИ ПОВИННІ БУТИ ВИДАЛЕНІ! ***
        /*
        private void Login_Click(object sender, RoutedEventArgs e) { ... }
        private void Register_Click(object sender, RoutedEventArgs e) { ... }
        */
    }
}