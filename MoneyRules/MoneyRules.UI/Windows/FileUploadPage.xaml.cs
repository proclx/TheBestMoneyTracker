using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MoneyRules.UI
{
    public partial class FileUploadPage : UserControl
    {
        public FileUploadPage()
        {
            InitializeComponent();
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true)
            {
                StatusText.Text = "Завантаження скасовано";
                return;
            }

            var path = openFileDialog.FileName;
            StatusText.Text = $"Опрацьовуємо файл: {path}";

            // Get current user from application properties
            if (!System.Windows.Application.Current.Properties.Contains("CurrentUser") || System.Windows.Application.Current.Properties["CurrentUser"] is not User currentUser)
            {
                MessageBox.Show("Спочатку потрібно увійти в систему.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Не вдалось знайти поточного користувача";
                return;
            }

            List<Transaction> transactions = new List<Transaction>();

            try
            {
                var lines = await Task.Run(() => File.ReadAllLines(path));

                if (lines.Length <= 1)
                {
                    StatusText.Text = "Файл пустий або містить лише заголовок.";
                    return;
                }

                // Expect header in first line
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // CSV format: Date,CategoryId,Amount,Type,Description
                    var parts = line.Split(',');
                    if (parts.Length < 5)
                    {
                        // skip or log malformed
                        continue;
                    }

                    DateTime date;
                    if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        // try parse with current culture
                        DateTime.TryParse(parts[0], out date);
                    }

                    // Ensure DateTime has Kind=Utc because PostgreSQL timestamptz requires UTC
                    if (date.Kind == DateTimeKind.Unspecified)
                    {
                        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }
                    else if (date.Kind == DateTimeKind.Local)
                    {
                        date = date.ToUniversalTime();
                    }

                    if (!int.TryParse(parts[1], out var categoryId))
                        continue;

                    if (!decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    {
                        // try with current culture
                        if (!decimal.TryParse(parts[2], out amount))
                            continue;
                    }

                    TransactionType type;
                    var typeStr = parts[3].Trim();
                    if (!Enum.TryParse<TransactionType>(typeStr, true, out type))
                    {
                        // try mapping common words
                        if (typeStr.Equals("Income", StringComparison.OrdinalIgnoreCase) || typeStr.Equals("Доход", StringComparison.OrdinalIgnoreCase) || typeStr.Equals("Дохід", StringComparison.OrdinalIgnoreCase))
                            type = TransactionType.Income;
                        else
                            type = TransactionType.Expense;
                    }

                    var description = string.Join(',', parts.Skip(4)).Trim();

                    transactions.Add(new Transaction
                    {
                        UserId = currentUser.UserId,
                        CategoryId = categoryId,
                        Amount = amount,
                        Type = type,
                        Date = date,
                        Description = description
                    });
                }

                if (transactions.Count == 0)
                {
                    StatusText.Text = "У файлі не знайдено валідних транзакцій.";
                    return;
                }

                // Save to DB using DI scope
                var app = (App)System.Windows.Application.Current;
                using (var scope = app.ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Ensure categories exist for the current user. If not, create placeholder categories.
                    var referencedCategoryIds = transactions.Select(t => t.CategoryId).Distinct().ToList();
                    var existingCategories = db.Categories.Where(c => c.UserId == currentUser.UserId && referencedCategoryIds.Contains(c.CategoryId)).Select(c => c.CategoryId).ToList();
                    var missing = referencedCategoryIds.Except(existingCategories).ToList();

                    // For missing CSV category ids, create placeholder categories and remember mapping
                    var createdCategoryMap = new Dictionary<int, int>(); // originalCsvId -> newCategoryId
                    var createdCategories = new List<MoneyRules.Domain.Entities.Category>();
                    foreach (var missingId in missing)
                    {
                        var cat = new MoneyRules.Domain.Entities.Category
                        {
                            UserId = currentUser.UserId,
                            Name = $"Imported Category {missingId}",
                            Type = MoneyRules.Domain.Enums.CategoryType.Category1
                        };
                        createdCategories.Add(cat);
                        db.Categories.Add(cat);
                    }

                    await db.SaveChangesAsync(); // save new categories first to get their generated IDs

                    // Build mapping from original CSV ids to newly created category IDs
                    for (int i = 0; i < missing.Count; i++)
                    {
                        var originalId = missing[i];
                        var createdCat = createdCategories[i];
                        createdCategoryMap[originalId] = createdCat.CategoryId;
                    }

                    // Remap transactions that referenced missing category ids to the new category ids
                    foreach (var t in transactions)
                    {
                        if (createdCategoryMap.TryGetValue(t.CategoryId, out var newCatId))
                        {
                            t.CategoryId = newCatId;
                        }
                    }

                    // Attach transactions and save
                    await db.Transactions.AddRangeAsync(transactions);
                    await db.SaveChangesAsync();
                }

                StatusText.Text = $"Успішно імпортовано {transactions.Count} транзакцій.";
                MessageBox.Show($"Імпортовано {transactions.Count} транзакцій.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                StatusText.Text = "Помилка під час збереження в БД.";
                var inner = dbEx.InnerException;
                var message = dbEx.Message;
                while (inner != null)
                {
                    message += "\nInner: " + inner.Message;
                    inner = inner.InnerException;
                }
                MessageBox.Show(message, "Помилка збереження в БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Помилка під час імпорту файлу.";
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message += "\nInner: " + inner.Message;
                    inner = inner.InnerException;
                }
                MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
