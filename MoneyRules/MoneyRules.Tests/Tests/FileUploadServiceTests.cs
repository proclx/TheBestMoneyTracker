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
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _dbContext;
        private readonly string _testUserId = "1";

        public FileUploadServiceTests()
        {
            var services = new ServiceCollection();
            
            // Налаштовуємо тестову базу даних
            var dbContextFactory = new TestDbContextFactory();
            _dbContext = dbContextFactory.CreateDbContext();
            services.AddSingleton(_dbContext);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task ProcessFileAsync_DefaultFormat_ShouldImportTransactions()
        {
            // Arrange
            var service = new FileUploadService(_serviceProvider);
            var testFilePath = CreateTestCsvFile(false);
            var userId = 1;

            try
            {
                // Act
                var result = await service.ProcessFileAsync(testFilePath, userId, false);

                // Assert
                Assert.True(result.success);
                Assert.True(result.count > 0);
                
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
        public async Task ProcessFileAsync_MonobankFormat_ShouldImportTransactions()
        {
            // Arrange
            var service = new FileUploadService(_serviceProvider);
            var testFilePath = CreateTestCsvFile(true);
            var userId = 1;

            try
            {
                // Act
                var result = await service.ProcessFileAsync(testFilePath, userId, true);

                // Assert
                Assert.True(result.success);
                Assert.True(result.count > 0);
                
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
        public async Task ProcessFileAsync_EmptyFile_ShouldReturnError()
        {
            // Arrange
            var service = new FileUploadService(_serviceProvider);
            var testFilePath = Path.GetTempFileName();
            File.WriteAllText(testFilePath, "Date,CategoryId,Amount,Type,Description\n");
            var userId = 1;

            try
            {
                // Act
                var result = await service.ProcessFileAsync(testFilePath, userId, false);

                // Assert
                Assert.False(result.success);
                Assert.Contains("empty", result.message.ToLower());
                Assert.Equal(0, result.count);
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
        public async Task ProcessFileAsync_InvalidFile_ShouldReturnError()
        {
            // Arrange
            var service = new FileUploadService(_serviceProvider);
            var testFilePath = Path.GetTempFileName();
            File.WriteAllText(testFilePath, "Invalid,CSV,Format\n1,2,3");
            var userId = 1;

            try
            {
                // Act
                var result = await service.ProcessFileAsync(testFilePath, userId, false);

                // Assert
                Assert.False(result.success);
                Assert.Equal(0, result.count);
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
    }
}