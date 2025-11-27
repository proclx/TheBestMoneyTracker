using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MoneyRules.UI.ViewModel
{
    public class FileUploadViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private string _statusText = "Оберіть файл для завантаження";
        private bool _isBusy = false;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public FileUploadViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            UploadDefaultCommand = new RelayCommand(async _ => await ExecuteUploadDefaultAsync(), _ => !IsBusy);
            UploadMonobankCommand = new RelayCommand(async _ => await ExecuteUploadMonobankAsync(), _ => !IsBusy);
            UploadPrivatCommand = new RelayCommand(async _ => await ExecuteUploadPrivatAsync(), _ => !IsBusy);
        }

        public ICommand UploadDefaultCommand { get; }
        public ICommand UploadMonobankCommand { get; }
        public ICommand UploadPrivatCommand { get; }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                // Use the Utils implementation of RelayCommand which exposes RaiseCanExecuteChanged
                try
                {
                    ((MoneyRules.UI.Utils.RelayCommand)UploadDefaultCommand).RaiseCanExecuteChanged();
                    ((MoneyRules.UI.Utils.RelayCommand)UploadMonobankCommand).RaiseCanExecuteChanged();
                    ((MoneyRules.UI.Utils.RelayCommand)UploadPrivatCommand).RaiseCanExecuteChanged();
                }
                catch { CommandManager.InvalidateRequerySuggested(); }
            }
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        private int? GetCurrentUserId()
        {
            if (System.Windows.Application.Current?.Properties["CurrentUser"] is MoneyRules.Domain.Entities.User u)
                return u.UserId;
            return null;
        }

        private async Task ExecuteUploadDefaultAsync()
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) { StatusText = "Завантаження скасовано"; return; }
            var path = dlg.FileName;
            var uid = GetCurrentUserId();
            if (!uid.HasValue) { StatusText = "Користувач не знайдений"; return; }

            try
            {
                IsBusy = true; StatusText = $"Опрацьовуємо файл: {path}";
                var lines = await Task.Run(() => System.IO.File.ReadAllLines(path));
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();
                    var result = await scopedService.ImportDefaultCsvAsync(lines, uid.Value);
                    StatusText = result.Success ? $"Успішно імпортовано {result.ImportedCount} транзакцій." : $"Помилка імпорту: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = "Критична помилка під час обробки файлу: " + ex.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task ExecuteUploadMonobankAsync()
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) { StatusText = "Завантаження скасовано"; return; }
            var path = dlg.FileName;
            var uid = GetCurrentUserId();
            if (!uid.HasValue) { StatusText = "Користувач не знайдений"; return; }

            try
            {
                IsBusy = true; StatusText = $"Опрацьовуємо файл: {path}";
                var lines = await Task.Run(() => System.IO.File.ReadAllLines(path));
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();
                    var result = await scopedService.ImportMonobankCsvAsync(lines, uid.Value);
                    StatusText = result.Success ? $"Успішно імпортовано {result.ImportedCount} транзакцій." : $"Помилка імпорту: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = "Критична помилка під час обробки файлу: " + ex.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task ExecuteUploadPrivatAsync()
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*" };
            if (dlg.ShowDialog() != true) { StatusText = "Завантаження скасовано"; return; }
            var path = dlg.FileName;
            var uid = GetCurrentUserId();
            if (!uid.HasValue) { StatusText = "Користувач не знайдений"; return; }

            try
            {
                IsBusy = true; StatusText = $"Опрацьовуємо файл: {path}";
                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();
                    var result = await scopedService.ImportPrivatBankXlsxAsync(path, uid.Value);
                    StatusText = result.Success ? $"Успішно імпортовано {result.ImportedCount} транзакцій." : $"Помилка імпорту: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = "Критична помилка під час обробки файлу: " + ex.Message;
            }
            finally { IsBusy = false; }
        }
    }
}
