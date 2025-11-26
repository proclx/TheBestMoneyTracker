using System.Windows;
using System.Windows.Input;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.ViewModel;

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;

        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            // Resolve ViewModel with DI-provided IAuthService
            DataContext = new LoginViewModel(_authService);
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                var method = vm.GetType().GetMethod("ExecuteForgotPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(vm, null);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}

