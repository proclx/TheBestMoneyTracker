using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Xunit;

namespace MoneyRules.Tests.Tests
{
    public class TransactionFilterTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            // Початкові дані для перевірки
            context.Categories.Add(new Category
            {
                CategoryId = 1,
                Name = "Food",
                UserId = 10
            });

            context.Transactions.Add(new Transaction
            {
                TransactionId = 1,
                UserId = 10,
                Amount = 100,
                Description = "Test transaction"
            });

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldAddNewCategory_WhenNotExists()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var newCategory = new Category
            {
                Name = "Travel",
                UserId = 10
            };

            // Act
            var result = await service.CreateCategoryAsync(newCategory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Travel", result.Name);
            Assert.Equal(2, await context.Categories.CountAsync()); // Додалася друга категорія
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnExistingCategory_WhenAlreadyExists()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var existingCategory = new Category
            {
                Name = "Food",
                UserId = 10
            };

            // Act
            var result = await service.CreateCategoryAsync(existingCategory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CategoryId); // Існуюча категорія збережена
            Assert.Equal(1, await context.Categories.CountAsync()); // Дублікат не створено
        }
    }
}
