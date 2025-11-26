using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.UI.Utils; 

namespace MoneyRules.UI.ViewModels
{
    // INotifyPropertyChanged повідомляє інтерфейсу, що дані змінилися
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly IUserProfileService _profileService;
        private readonly User _currentUser;

        // Поле для зберігання значення
        private decimal _monthlyBudget;

        // Конструктор приймає сервіси (Dependency Injection)
        public DashboardViewModel(IUserProfileService profileService, User currentUser)
        {
            _profileService = profileService;
            _currentUser = currentUser;

            // 1. Завантажуємо початкове значення бюджету
            if (_currentUser.Settings != null)
            {
                MonthlyBudget = _currentUser.Settings.MonthlyBudget;
            }

            // 2. Ініціалізуємо команду
            SaveBudgetCommand = new RelayCommand(ExecuteSaveBudget);
        }

        // Властивість для Binding (Прив'язки) у XAML
        public decimal MonthlyBudget
        {
            get => _monthlyBudget;
            set
            {
                _monthlyBudget = value;
                OnPropertyChanged(); // Оновлює UI автоматично
            }
        }

        // Команда, до якої прив'яжеться кнопка
        public ICommand SaveBudgetCommand { get; }

        // Метод, який виконається при натисканні кнопки
        private void ExecuteSaveBudget(object obj)
        {
            try
            {
                // Викликаємо ваш старий добрий сервіс
                _profileService.UpdateUserBudget(_currentUser.UserId, MonthlyBudget);

                // Оновлюємо локального користувача
                if (_currentUser.Settings == null)
                    _currentUser.Settings = new Settings { UserId = _currentUser.UserId };

                _currentUser.Settings.MonthlyBudget = MonthlyBudget;

                MessageBox.Show($"Бюджет {MonthlyBudget} успішно збережено!", "MVVM Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        // Стандартна реалізація інтерфейсу сповіщень
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}