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
                    // Save current user to application for other pages to access
                    var app = (App)System.Windows.Application.Current;
                    // Store the logged-in user in Application Properties so other controls can access it
                    System.Windows.Application.Current.Properties["CurrentUser"] = user;

                    MessageBox.Show($"Вітаємо, {user.Name}!", "Успішний вхід", MessageBoxButton.OK, MessageBoxImage.Information);

                    var dashboard = app.ServiceProvider.GetRequiredService<MainWindow>();
                    dashboard.Show();

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
