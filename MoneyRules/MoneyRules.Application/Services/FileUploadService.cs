using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyRules.Application.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly AppDbContext _db;


        public FileUploadService(AppDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<ImportResult> ImportDefaultCsvAsync(string[] lines, int userId)
        {
            try
            {
                var transactions = ParseDefaultCsv(lines, userId);
                if (transactions.Count == 0)
                {
                    return new ImportResult { Success = false, ErrorMessage = "У файлі не знайдено валідних транзакцій (Default)." };
                }

                await SaveTransactionsAsync(transactions, userId);

                return new ImportResult { Success = true, ImportedCount = transactions.Count };
            }
            catch (Exception ex)
            {
                // Логування помилки
                return new ImportResult { Success = false, ErrorMessage = $"Помилка: {ex.Message}" };
            }
        }

        public async Task<ImportResult> ImportMonobankCsvAsync(string[] lines, int userId)
        {
            try
            {
                var transactions = ParseMonobankCsv(lines, userId);
                if (transactions.Count == 0)
                {
                    return new ImportResult { Success = false, ErrorMessage = "У файлі не знайдено валідних транзакцій (Monobank)." };
                }
                
                await SaveTransactionsAsync(transactions, userId);

                return new ImportResult { Success = true, ImportedCount = transactions.Count };
            }
            catch (Exception ex)
            {
                // Логування помилки
                return new ImportResult { Success = false, ErrorMessage = $"Помилка: {ex.Message}" };
            }
        }

        // --- ЛОГІКА ЗБЕРЕЖЕННЯ (спільна для обох) ---
        private async Task SaveTransactionsAsync(List<Transaction> transactions, int userId)
        {
            var referencedCategoryIds = transactions.Select(t => t.CategoryId).Distinct().ToList();
            
            var existingCategories = await _db.Categories
                .Where(c => c.UserId == userId && referencedCategoryIds.Contains(c.CategoryId))
                .Select(c => c.CategoryId)
                .ToListAsync();
            
            var missing = referencedCategoryIds.Except(existingCategories).ToList();

            var createdCategoryMap = new Dictionary<int, int>();
            var createdCategories = new List<Category>();
            
            foreach (var missingId in missing)
            {
                var cat = new Category
                {
                    UserId = userId,
                    Name = $"Imported Category {missingId}",
                    Type = CategoryType.Category1 // Або будь-який тип за замовчуванням
                };
                createdCategories.Add(cat);
                _db.Categories.Add(cat);
            }

            // Зберігаємо нові категорії, щоб отримати їх ID
            if (createdCategories.Count > 0)
            {
                await _db.SaveChangesAsync();
            }

            // Створюємо мапу: [ID з файлу] -> [Новий ID з БД]
            for (int i = 0; i < missing.Count; i++)
            {
                var originalId = missing[i];
                var createdCat = createdCategories[i];
                createdCategoryMap[originalId] = createdCat.CategoryId;
            }

            // Оновлюємо ID категорій у транзакціях
            foreach (var t in transactions)
            {
                if (createdCategoryMap.TryGetValue(t.CategoryId, out var newCatId))
                {
                    t.CategoryId = newCatId;
                }
            }

            // Зберігаємо транзакції
            await _db.Transactions.AddRangeAsync(transactions);
            await _db.SaveChangesAsync();
        }



        private List<Transaction> ParseDefaultCsv(string[] lines, int userId)
        {
            var transactions = new List<Transaction>();
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 5) continue;

                if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    DateTime.TryParse(parts[0], out date);
                }
                if (date.Kind == DateTimeKind.Unspecified) date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                else if (date.Kind == DateTimeKind.Local) date = date.ToUniversalTime();

                if (!int.TryParse(parts[1], out var categoryId)) continue;

                if (!decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    if (!decimal.TryParse(parts[2], out amount)) continue;
                }

                TransactionType type;
                var typeStr = parts[3].Trim();
                if (!Enum.TryParse<TransactionType>(typeStr, true, out type))
                {
                    type = (typeStr.Equals("Income", StringComparison.OrdinalIgnoreCase) || typeStr.Equals("Доход", StringComparison.OrdinalIgnoreCase) || typeStr.Equals("Дохід", StringComparison.OrdinalIgnoreCase))
                        ? TransactionType.Income
                        : TransactionType.Expense;
                }

                var description = string.Join(',', parts.Skip(4)).Trim();
                transactions.Add(new Transaction
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Amount = Math.Abs(amount),
                    Type = type,
                    Date = date,
                    Description = description
                });
            }
            return transactions;
        }

        private List<Transaction> ParseMonobankCsv(string[] lines, int userId)
        {
            var transactions = new List<Transaction>();
            var numberCulture = CultureInfo.InvariantCulture;
            var dateCulture = new CultureInfo("uk-UA");

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                var dateString = parts[0].Trim('"');
                if (!DateTime.TryParseExact(dateString, "dd.MM.yyyy HH:mm:ss", dateCulture, DateTimeStyles.None, out var date))
                {
                    if (!DateTime.TryParse(dateString, dateCulture, out date)) continue;
                }
                if (date.Kind == DateTimeKind.Unspecified) date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                else if (date.Kind == DateTimeKind.Local) date = date.ToUniversalTime();

                var description = parts[1].Trim('"');
                
                var mccString = parts[2].Trim('"');
                if (!int.TryParse(mccString, out var categoryId))
                {
                    categoryId = 0; // "Без категорії"
                }

                var amountString = parts[3].Trim('"');
                if (!decimal.TryParse(amountString, NumberStyles.Any, numberCulture, out var amount))
                {
                    continue;
                }

                var type = (amount >= 0) ? TransactionType.Income : TransactionType.Expense;

                transactions.Add(new Transaction
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Amount = Math.Abs(amount),
                    Type = type,
                    Date = date,
                    Description = description
                });
            }
            return transactions;
        }
    }
}