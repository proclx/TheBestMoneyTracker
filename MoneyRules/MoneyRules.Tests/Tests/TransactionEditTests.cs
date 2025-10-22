using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoneyRules.Tests.Tests
{
    public class TransactionServiceTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            // Наповнимо початковими даними
            var user = new User
            {
                UserId = 1,
                Name = "TestUser",
                Email = "test@mail.com",
                PasswordHash = "hash123",
                ProfilePhoto = Array.Empty<byte>()
            };

            var cat1 = new Category { CategoryId = 1, Name = "Food", UserId = 1 };
            var cat2 = new Category { CategoryId = 2, Name = "Salary", UserId = 1 };

            var t1 = new Transaction
            {
                TransactionId = 1,
                UserId = 1,
                CategoryId = 1,
                Amount = 100,
                Type = TransactionType.Expense,
                Date = DateTime.UtcNow.AddDays(-2),
                Description = "Groceries"
            };

            var t2 = new Transaction
            {
                TransactionId = 2,
                UserId = 1,
                CategoryId = 2,
                Amount = 2000,
                Type = TransactionType.Income,
                Date = DateTime.UtcNow.AddDays(-1),
                Description = "Salary"
            };

            context.Users.Add(user);
            context.Categories.AddRange(cat1, cat2);
            context.Transactions.AddRange(t1, t2);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task GetTransactionsAsync_Returns_All_For_User()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var filter = new TransactionFilter { UserId = 1 };
            var result = await service.GetTransactionsAsync(filter);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetTransactionsAsync_Filters_By_Category()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var filter = new TransactionFilter { UserId = 1, CategoryId = 1 };
            var result = await service.GetTransactionsAsync(filter);

            Assert.Single(result);
            Assert.Equal("Groceries", result.First().Description);
        }

        [Fact]
        public async Task UpdateAsync_Changes_Transaction_Data()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var transaction = await context.Transactions.FirstAsync(t => t.TransactionId == 1);
            transaction.Amount = 999;
            transaction.Description = "Updated";

            await service.UpdateAsync(transaction);

            var updated = await context.Transactions.FindAsync(1);
            Assert.Equal(999, updated.Amount);
            Assert.Equal("Updated", updated.Description);
        }

        [Fact]
        public async Task GetUserCategoriesAsync_Returns_Correct_List()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var result = await service.GetUserCategoriesAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Food");
            Assert.Contains(result, c => c.Name == "Salary");
        }

        [Fact]
        public async Task GetTransactionsAsync_Filters_By_DateRange()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var from = DateTime.UtcNow.AddDays(-1.5);
            var to = DateTime.UtcNow;

            var filter = new TransactionFilter { UserId = 1, FromDate = from, ToDate = to };
            var result = await service.GetTransactionsAsync(filter);

            Assert.Single(result);
            Assert.Equal("Salary", result.First().Description);
        }
    }
}
