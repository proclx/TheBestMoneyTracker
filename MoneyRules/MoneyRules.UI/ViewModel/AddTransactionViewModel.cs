using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.UI.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Linq;

namespace MoneyRules.UI.ViewModels
{
    public class AddTransactionViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionService _transactionService;
        private const int DummyUserId = 1;

        public ObservableCollection<Category> AvailableCategories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<TransactionType> TransactionTypes { get; set; }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); ((RelayCommand)SaveTransactionCommand).RaiseCanExecuteChanged(); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); ((RelayCommand)SaveTransactionCommand).RaiseCanExecuteChanged(); }
        }

        private TransactionType _selectedType;
        public TransactionType SelectedType
        {
            get => _selectedType;
            set { _selectedType = value; OnPropertyChanged(); }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set { _isSaving = value; OnPropertyChanged(); ((RelayCommand)SaveTransactionCommand).RaiseCanExecuteChanged(); }
        }

        public ICommand SaveTransactionCommand { get; }

        public AddTransactionViewModel(ITransactionService transactionService)
        {
            _transactionService = transactionService;

            TransactionTypes = new ObservableCollection<TransactionType>(Enum.GetValues<TransactionType>());
            SelectedType = TransactionTypes.FirstOrDefault();

            SaveTransactionCommand = new RelayCommand(ExecuteSaveTransaction, CanExecuteSaveTransaction);

            _ = ExecuteLoadDataAsync();
        }

        private async Task ExecuteLoadDataAsync()
        {
            StatusMessage = "Завантаження категорій...";
            try
            {
                var categories = await _transactionService.GetUserCategoriesAsync(DummyUserId);
                AvailableCategories.Clear();
                foreach (var cat in categories)
                {
                    AvailableCategories.Add(cat);
                }
                SelectedCategory = AvailableCategories.FirstOrDefault();
                StatusMessage = "Готово до додавання транзакції.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка завантаження: {ex.Message}";
            }
        }

        private bool CanExecuteSaveTransaction(object parameter)
        {
            return Amount > 0 && SelectedCategory != null && !IsSaving;
        }

        private async void ExecuteSaveTransaction(object parameter)
        {
            IsSaving = true;
            StatusMessage = string.Empty;

            try
            {
                var newTransaction = new Transaction
                {
                    UserId = DummyUserId,
                    Amount = Amount,
                    CategoryId = SelectedCategory?.CategoryId ?? 0,
                    Type = SelectedType,
                    Date = Date,
                    Description = Description
                };

                var result = await _transactionService.AddTransactionAsync(newTransaction);

                StatusMessage = $"Транзакція {result.TransactionId} успішно збережена!";

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка збереження: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}