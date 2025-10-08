using System;
using System.Windows;
using MoneyRules.Application.Services;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.UI.Windows
{
    public partial class RegisterWindow : Window
    {
        private readonly IRegistrationService _authService;

        public RegisterWindow()
        {
            InitializeComponent();
            var db = new AppDbContextFactory().CreateDbContext(null);
            _authService = new RegistrationService(db);
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
