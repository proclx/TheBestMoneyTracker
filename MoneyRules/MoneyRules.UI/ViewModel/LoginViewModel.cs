using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.Windows;

namespace MoneyRules.UI.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync(), _ => CanExecuteLogin());
            ConfirmTwoFactorCommand = new RelayCommand(async _ => await ExecuteConfirmTwoFactorAsync(), _ => CanConfirmTwoFactor());
            BackCommand = new RelayCommand(_ => ExecuteBack());
            ForgotPasswordCommand = new RelayCommand(_ => ExecuteForgotPassword());
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(nameof(RememberMe)); }
        }

        private bool _isTwoFactorVisible;
        public bool IsTwoFactorVisible
        {
            get => _isTwoFactorVisible;
            set { _isTwoFactorVisible = value; OnPropertyChanged(nameof(IsTwoFactorVisible)); }
        }

        private string _twoFactorCode = string.Empty;
        public string TwoFactorCode
        {
            get => _twoFactorCode;
            set { _twoFactorCode = value; OnPropertyChanged(nameof(TwoFactorCode)); }
        }

        public ICommand LoginCommand { get; }
        public ICommand ConfirmTwoFactorCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        private bool CanExecuteLogin() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        private bool CanConfirmTwoFactor() => !string.IsNullOrWhiteSpace(TwoFactorCode) && !string.IsNullOrWhiteSpace(Email);

        private async Task ExecuteLoginAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Email та пароль не можуть бути порожніми.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _authService.LoginAsync(Email.Trim(), Password, RememberMe);

                if (result.IsSuccess && result.User != null)
                {
                    var code = _authService.GenerateTwoFactorCode(Email.Trim());
                    MessageBox.Show($"Ваш код 2FA: {code}", "Двофакторна автентифікація", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsTwoFactorVisible = true;
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "Невірний email або пароль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка при вході: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteConfirmTwoFactorAsync()
        {
            try
            {
                if (!_authService.VerifyTwoFactorCode(Email.Trim(), TwoFactorCode.Trim()))
                {
                    MessageBox.Show("Невірний код 2FA.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _authService.RemoveTwoFactorCode(Email.Trim());

                var loginResult = await _authService.LoginAsync(Email.Trim(), Password, RememberMe);
                if (loginResult.IsSuccess && loginResult.User != null)
                {
                    System.Windows.Application.Current.Properties["CurrentUser"] = loginResult.User;

                    if (RememberMe && !string.IsNullOrEmpty(loginResult.RememberMeToken))
                    {
                        try
                        {
                            var saveType = Type.GetType("MoneyRules.UI.Utils.SecureTokenStorage, MoneyRules.UI");
                            if (saveType != null)
                            {
                                var mi = saveType.GetMethod("SaveToken");
                                mi?.Invoke(null, new object[] { loginResult.RememberMeToken });
                            }
                        }
                        catch { /* ignore token save errors */ }
                    }

                    var mainWindow = App.GetService<MainWindow>();
                    if (mainWindow != null)
                    {
                        mainWindow.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Помилка входу після підтвердження 2FA.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteBack()
        {
            var welcome = App.GetService<WelcomeWindow>();
            if (welcome != null)
            {
                welcome.Show();
            }
        }

        private void ExecuteForgotPassword()
        {
            var window = new ForgotPasswordWindow(_authService);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}