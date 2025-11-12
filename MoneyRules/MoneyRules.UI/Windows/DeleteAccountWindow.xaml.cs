using System.Windows; 
using MoneyRules.Application.Interfaces;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.UI.Windows
{
    public partial class DeleteAccountWindow : Window
    {
        private readonly IAuthService _authService;

        public DeleteAccountWindow()
        {
            InitializeComponent();

            // Створюємо DbContext і сервіс
            var context = new AppDbContext(); // або через DI, якщо налаштовано
            _authService = new AuthService(context);
        }

        private async void ConfirmDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Викликаємо метод видалення акаунту
            bool success = await _authService.DeleteUserAccountAsync(email, password);

            if (success)
            {
                MessageBox.Show("Your account has been deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закриваємо додаток після видалення
                System.Windows.Application.Current.Shutdown(); 
            }
            else
            {
                MessageBox.Show("Invalid email or password. Account not deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

