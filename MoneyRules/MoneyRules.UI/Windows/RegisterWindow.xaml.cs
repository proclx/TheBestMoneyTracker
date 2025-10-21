using System.Windows;
using MoneyRules.Application.Interfaces;

namespace MoneyRules.UI.Windows
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthService _authService;

        public RegisterWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = NameBox.Text;
                var email = EmailBox.Text;
                var password = PasswordBox.Password;

                var user = await _authService.RegisterAsync(name, email, password);
                MessageBox.Show($"Користувач {user.Name} зареєстрований успішно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
