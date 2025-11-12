using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Enums;
using System.Linq;
using System.Collections.Generic; // <-- ДОДАНО
using OfficeOpenXml; // <-- ДОДАНО

namespace MoneyRules.Tests.Tests
{
    public class FileUploadServiceTests
    {
        private readonly AppDbContext _dbContext;

        public FileUploadServiceTests()
        {
            // Use the static test factory
            _dbContext = TestDbContextFactory.CreateInMemoryDb();
            // Потрібно для EPPlus 7+
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public async Task ImportDefaultCsvAsync_ShouldImportTransactions()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var testFilePath = CreateTestCsvFile(false);
            var userId = 1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportDefaultCsvAsync(lines, userId);

                // Assert
                Assert.True(result.Success);
                Assert.True(result.ImportedCount > 0);

                // Перевіряємо, що транзакції додані в базу даних
                var transactions = _dbContext.Transactions
                    .Where(t => t.UserId == userId)
                    .ToList();

                Assert.NotEmpty(transactions);
                Assert.All(transactions, t =>
                {
                    Assert.Equal(userId, t.UserId);
                    Assert.NotEqual(0, t.Amount);
                    Assert.NotEqual(default, t.Date);
                });
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Fact]
        public async Task ImportMonobankCsvAsync_ShouldImportTransactions()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var testFilePath = CreateTestCsvFile(true);
            var userId = 1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportMonobankCsvAsync(lines, userId);

                // Assert
                Assert.True(result.Success);
                Assert.True(result.ImportedCount > 0);

                var transactions = _dbContext.Transactions
                    .Where(t => t.UserId == userId)
                    .ToList();

                Assert.NotEmpty(transactions);
                Assert.All(transactions, t =>
                {
                    Assert.Equal(userId, t.UserId);
                    Assert.NotEqual(0, t.Amount);
                    Assert.NotEqual(default, t.Date);
                });
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Fact]
        public async Task ImportDefaultCsvAsync_EmptyFile_ShouldReturnError()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var testFilePath = Path.GetTempFileName();
            File.WriteAllText(testFilePath, "Date,CategoryId,Amount,Type,Description\n");
            var userId = 1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportDefaultCsvAsync(lines, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Contains("валідних", result.ErrorMessage ?? string.Empty);
                Assert.Equal(0, result.ImportedCount);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Fact]
        public async Task ImportDefaultCsvAsync_InvalidFile_ShouldReturnError()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var testFilePath = Path.GetTempFileName();
            File.WriteAllText(testFilePath, "Invalid,CSV,Format\n1,2,3");
            var userId = 1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportDefaultCsvAsync(lines, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal(0, result.ImportedCount);
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }


        // НОВІ ТЕСТИ ДЛЯ ПРИВАТБАНК XLSX
 

        [Fact]
        public async Task ImportPrivatBankXlsxAsync_ShouldCreateNewCategories_WhenTheyDoNotExist()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var userId = 1;
            var fileData = new List<object[]>
            {
                // Дата, Категорія, Опис, Сума
                new object[] { "10.11.2025 10:00:00", "Кафе", "Кава", "-100,00 UAH" },
                new object[] { "11.11.2025 12:00:00", "Зарплата", "Надходження", "20000 UAH" }
            };
            var testFilePath = CreateTestXlsxFile(fileData);

            try
            {
                // Act
                var result = await service.ImportPrivatBankXlsxAsync(testFilePath, userId);

                // Assert
                Assert.True(result.Success);
                Assert.Equal(2, result.ImportedCount);

                // Перевіряємо, що категорії були створені
                var categories = _dbContext.Categories.ToList();
                Assert.Equal(2, categories.Count);
                Assert.Contains(categories, c => c.Name == "Кафе");
                Assert.Contains(categories, c => c.Name == "Зарплата");

                // Перевіряємо, що транзакції були створені
                var transactions = _dbContext.Transactions.ToList();
                Assert.Equal(2, transactions.Count);

                var expense = transactions.First(t => t.Type == TransactionType.Expense);
                Assert.Equal(100, expense.Amount);
                Assert.Equal("Кава", expense.Description);
                Assert.Equal(categories.First(c => c.Name == "Кафе").CategoryId, expense.CategoryId);

                var income = transactions.First(t => t.Type == TransactionType.Income);
                Assert.Equal(20000, income.Amount);
                Assert.Equal("Надходження", income.Description);
                Assert.Equal(categories.First(c => c.Name == "Зарплата").CategoryId, income.CategoryId);
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [Fact]
        public async Task ImportPrivatBankXlsxAsync_ShouldUseExistingCategory_WhenItExists()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var userId = 1;

            // Додаємо категорію в базу "до" тесту
            var existingCategory = new Category { Name = "Кафе", UserId = userId, Type = CategoryType.Category1 };
            _dbContext.Categories.Add(existingCategory);
            _dbContext.SaveChanges(); // Зберігаємо, щоб отримати ID
            var existingCatId = existingCategory.CategoryId;

            var fileData = new List<object[]>
            {
                new object[] { "10.11.2025 10:00:00", "Кафе", "Сніданок", "-250,00 UAH" }
            };
            var testFilePath = CreateTestXlsxFile(fileData);

            try
            {
                // Act
                var result = await service.ImportPrivatBankXlsxAsync(testFilePath, userId);

                // Assert
                Assert.True(result.Success);
                Assert.Equal(1, result.ImportedCount);

                // Перевіряємо, що нова категорія НЕ була створена
                var categories = _dbContext.Categories.ToList();
                Assert.Single(categories); // Має бути лише одна категорія (та, що ми додали)

                // Перевіряємо, що транзакція посилається на існуючу категорію
                var transaction = _dbContext.Transactions.First();
                Assert.Equal(existingCatId, transaction.CategoryId);
                Assert.Equal(250, transaction.Amount);
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [Fact]
        public async Task ImportPrivatBankXlsxAsync_EmptyFile_ShouldReturnError()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var userId = 1;
            var fileData = new List<object[]>(); // Без даних
            var testFilePath = CreateTestXlsxFile(fileData);

            try
            {
                // Act
                var result = await service.ImportPrivatBankXlsxAsync(testFilePath, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Contains("не знайдено валідних транзакцій", result.ErrorMessage);
                Assert.Equal(0, _dbContext.Transactions.Count());
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }



        private string CreateTestCsvFile(bool isMonobank)
        {
            var filePath = Path.GetTempFileName();
            var content = new System.Text.StringBuilder();

            if (isMonobank)
            {
                content.AppendLine("Дата,Опис,MCC,Сума");
                content.AppendLine("\"01.11.2025 10:00:00\",\"Покупка в магазині\",\"5411\",\"-100.50\"");
                content.AppendLine("\"01.11.2025 11:00:00\",\"Зарплата\",\"0\",\"5000.00\"");
            }
            else
            {
                content.AppendLine("Date,CategoryId,Amount,Type,Description");
                content.AppendLine("2025-11-01,1,100.50,Expense,Test expense");
                content.AppendLine("2025-11-01,2,5000.00,Income,Test income");
            }

            File.WriteAllText(filePath, content.ToString());
            return filePath;
        }


        private string CreateTestXlsxFile(List<object[]> data)
        {

            var filePath = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
            
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Додаємо заголовок (бо парсер пропускає 1-й рядок)
                worksheet.Cells[1, 1].Value = "Дата";
                worksheet.Cells[1, 2].Value = "Категорія";
                worksheet.Cells[1, 3].Value = "Картка";
                worksheet.Cells[1, 4].Value ="Опис операції";
                worksheet.Cells[1, 5].Value = "Сума в валюті картки";

                // Додаємо дані
                for (int i = 0; i < data.Count; i++)
                {
                    int row = i + 2; // Починаємо з 2-го рядка
                    worksheet.Cells[row, 1].Value = data[i][0]; // Дата
                    worksheet.Cells[row, 2].Value = data[i][1]; // Категорія
                    worksheet.Cells[row, 4].Value = data[i][2]; // Опис
                    worksheet.Cells[row, 5].Value = data[i][3]; // Сума
                }
                package.Save();
            }
            return filePath;
        }
    }
}