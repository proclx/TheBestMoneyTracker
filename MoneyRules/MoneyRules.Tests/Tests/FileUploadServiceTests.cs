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

namespace MoneyRules.Tests.Tests
{
    public class FileUploadServiceTests
    {
        private readonly AppDbContext _dbContext;

        public FileUploadServiceTests()
        {
            // Use the static test factory
            _dbContext = TestDbContextFactory.CreateInMemoryDb();
        }

        [Fact]
        public async Task ImportDefaultCsvAsync_ShouldImportTransactions()
        {
            // Arrange
            var service = new FileUploadService(_dbContext);
            var testFilePath = CreateTestCsvFile(false);
            var userId =1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportDefaultCsvAsync(lines, userId);

                // Assert
                Assert.True(result.Success);
                Assert.True(result.ImportedCount >0);

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
            var userId =1;

            try
            {
                var lines = File.ReadAllLines(testFilePath);
                // Act
                var result = await service.ImportMonobankCsvAsync(lines, userId);

                // Assert
                Assert.True(result.Success);
                Assert.True(result.ImportedCount >0);

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
            var userId =1;

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
            var userId =1;

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

        private string CreateTestCsvFile(bool isMonobank)
        {
            var filePath = Path.GetTempFileName();
            var content = new System.Text.StringBuilder();

            if (isMonobank)
            {
                content.AppendLine("Дата,Опис,MCC,Сума");
                content.AppendLine("\"01.11.202510:00:00\",\"Покупка в магазині\",\"5411\",\"-100.50\"");
                content.AppendLine("\"01.11.202511:00:00\",\"Зарплата\",\"0\",\"5000.00\"");
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
    }
}