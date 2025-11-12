using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MoneyRules.Domain.Entities;
using MoneyRules.Application.Interfaces; 
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoneyRules.UI.Windows 
{
    public partial class FileUploadPage : UserControl
    {
        public FileUploadPage()
        {
            InitializeComponent();
        }

        private async void UploadDefaultFile_Click(object sender, RoutedEventArgs e)
        {
            await ProcessFileAsync(
                (lines, userId, service) => service.ImportDefaultCsvAsync(lines, userId)
            );
        }

        private async void UploadMonobankFile_Click(object sender, RoutedEventArgs e)
        {
            await ProcessFileAsync(
                (lines, userId, service) => service.ImportMonobankCsvAsync(lines, userId)
            );
        }
        
        // --- ОБРОБНИК ПРИВАТБАНКУ ---
        private async void UploadPrivatBankFile_Click(object sender, RoutedEventArgs e)
        {
            // 1. Оновлюємо фільтр, щоб він приймав .xlsx
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };
            
            if (openFileDialog.ShowDialog() != true)
            {
                StatusText.Text = "Завантаження скасовано";
                return;
            }

            var path = openFileDialog.FileName;
            StatusText.Text = $"Опрацьовуємо файл: {path}";

            // 2. Перевірка користувача
            if (System.Windows.Application.Current.Properties["CurrentUser"] is not User currentUser)
            {
                MessageBox.Show("Спочатку потрібно увійти в систему.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Не вдалось знайти поточного користувача";
                return;
            }
            
            // 3. Перевірка сервісів
            if (System.Windows.Application.Current is not App app || app.ServiceProvider == null)
            {
                MessageBox.Show("Помилка: Неможливо отримати доступ до сервісів додатка.", "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Помилка конфігурації додатка.";
                return;
            }

            try
            {
                using (var scope = app.ServiceProvider.CreateScope())
                {
                    var fileUploadService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();

                    // 4. Викликаємо новий метод XlsxAsync, передаючи шлях (path)
                    ImportResult result = await fileUploadService.ImportPrivatBankXlsxAsync(path, currentUser.UserId);

                    if (result.Success)
                    {
                        StatusText.Text = $"Успішно імпортовано {result.ImportedCount} транзакцій.";
                        MessageBox.Show($"Імпортовано {result.ImportedCount} транзакцій.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusText.Text = $"Помилка імпорту: {result.ErrorMessage}";
                        MessageBox.Show(result.ErrorMessage, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Критична помилка під час обробки файлу.";
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Цей метод залишається для CSV файлів (Default, Mono)
        private async Task ProcessFileAsync(Func<string[], int, IFileUploadService, Task<ImportResult>> importAction)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            
            if (openFileDialog.ShowDialog() != true)
            {
                StatusText.Text = "Завантаження скасовано";
                return;
            }

            var path = openFileDialog.FileName;
            StatusText.Text = $"Опрацьовуємо файл: {path}";

            if (System.Windows.Application.Current.Properties["CurrentUser"] is not User currentUser)
            {
                MessageBox.Show("Спочатку потрібно увійти в систему.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Не вдалось знайти поточного користувача";
                return;
            }

            try
            {
                // Читаємо файл як рядки (для CSV)
                var lines = await Task.Run(() => File.ReadAllLines(path));

                if (lines.Length <= 1)
                {
                    StatusText.Text = "Файл пустий або містить лише заголовок.";
                    return;
                }

                if (System.Windows.Application.Current is not App app || app.ServiceProvider == null)
                {
                    MessageBox.Show("Помилка: Неможливо отримати доступ до сервісів додатка.", "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Помилка конфігурації додатка.";
                    return;
                }

                using (var scope = app.ServiceProvider.CreateScope())
                {
                    var fileUploadService = scope.ServiceProvider.GetRequiredService<IFileUploadService>();
                    
                    // Викликаємо CSV-дію
                    ImportResult result = await importAction(lines, currentUser.UserId, fileUploadService);

                    if (result.Success)
                    {
                        StatusText.Text = $"Успішно імпортовано {result.ImportedCount} транзакцій.";
                        MessageBox.Show($"Імпортовано {result.ImportedCount} транзакцій.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusText.Text = $"Помилка імпорту: {result.ErrorMessage}";
                        MessageBox.Show(result.ErrorMessage, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Критична помилка під час обробки файлу.";
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}