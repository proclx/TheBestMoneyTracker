using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MoneyRules.Domain.Entities;
using MoneyRules.Application.Interfaces; // Імпорт інтерфейсу
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Collections.Generic; // Для Func

//
// КРИТИЧНЕ ВИПРАВЛЕННЯ: 
// Простір імен змінено на MoneyRules.UI.Windows
//
namespace MoneyRules.UI.Windows 
{
    public partial class FileUploadPage : UserControl
    {
        public FileUploadPage()
        {
            // Тепер InitializeComponent() буде знайдено
            InitializeComponent();
        }

        // Тепер UploadDefaultFile_Click буде знайдено
        private async void UploadDefaultFile_Click(object sender, RoutedEventArgs e)
        {
            await ProcessFileAsync(
                (lines, userId, service) => service.ImportDefaultCsvAsync(lines, userId)
            );
        }

        // Тепер UploadMonobankFile_Click буде знайдено
        private async void UploadMonobankFile_Click(object sender, RoutedEventArgs e)
        {
            await ProcessFileAsync(
                (lines, userId, service) => service.ImportMonobankCsvAsync(lines, userId)
            );
        }

        private async Task ProcessFileAsync(Func<string[], int, IFileUploadService, Task<ImportResult>> importAction)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            
            if (openFileDialog.ShowDialog() != true)
            {
                // Тепер StatusText буде знайдено
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