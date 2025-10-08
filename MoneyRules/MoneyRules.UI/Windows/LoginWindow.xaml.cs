using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Application.Services;
using Serilog;

namespace MoneyRules.UI.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;

        public LoginWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            try
            {
                var user = await _authService.LoginAsync(email, password);
                if (user != null)
                {
                    MessageBox.Show($"Вітаємо, {user.Name}!", "Успішний вхід", MessageBoxButton.OK, MessageBoxImage.Information);

                    // var app = (App)System.Windows.Application.Current;
                    // var dashboard = app.ServiceProvider.GetRequiredService<DashboardWindow>();
                    // dashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Невірний email або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Помилка під час входу користувача");
                MessageBox.Show("Сталася помилка при вході. Спробуйте пізніше.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
