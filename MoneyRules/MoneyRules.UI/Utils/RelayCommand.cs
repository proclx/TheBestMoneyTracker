using System;
using System.Windows.Input;

namespace MoneyRules.UI.Utils
{
    // Клас, що реалізує інтерфейс ICommand для зв'язування UI-елементів з ViewModel
    public class RelayCommand : ICommand
    {
        // Змінено на Action<object?> та Predicate<object?> для коректної обробки null
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute; // Додано '?'

        /// <summary>
        /// Створює нову команду з логікою виконання та перевірки активності.
        /// </summary>
        /// <param name="execute">Метод, який виконується при виклику команди.</param>
        /// <param name="canExecute">Метод, який визначає, чи активна кнопка.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) // Додано '?'
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Повідомляє UI про можливу зміну стану активності команди.
        /// </summary>
        public event EventHandler? CanExecuteChanged // Додано '?'
        {
            // Використовуємо CommandManager для автоматичної перевірки при зміні фокусу
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Визначає, чи може команда виконуватись.
        /// </summary>
        public bool CanExecute(object? parameter) // Додано '?'
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Виконує логіку команди.
        /// </summary>
        public void Execute(object? parameter) // Додано '?'
        {
            _execute(parameter);
        }
    }
}