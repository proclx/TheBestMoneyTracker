using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MoneyRules.Application.Services;
using Serilog;

namespace MoneyRules.UI
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;

        public MainWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            var user = await _authService.LoginAsync(email, password);
            if (user != null)
            {
                MessageBox.Show($"Welcome, {user.Name}!");
                // Тут можна відкривати головну сторінку
            }
            else
            {
                MessageBox.Show("Invalid email or password.");
            }
        }
    }
}