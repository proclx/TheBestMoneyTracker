using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using System;
using System.Collections.Generic;
using Xunit;
// --- ДОДАЙТЕ ЦІ РЯДКИ ---
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
// --- КІНЕЦЬ ДОДАВАННЯ ---

namespace MoneyRules.Tests.Tests
{
    public class AdviceServiceTests
    {
        // --- ДОДАНО ДОПОМІЖНИЙ МЕТОД ---
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
        // --- КІНЕЦЬ ДОДАВАННЯ ---

        [Fact]
        public void ReturnsNoTransactionsMessage_WhenEmpty()
        {
            // --- ЗМІНЕНО ТУТ ---
            var context = GetInMemoryDbContext();
            var service = new AdviceService(context);
            // --- КІНЕЦЬ ЗМІН ---

            var tips = service.GetAdvice(new List<Transaction>());

            Assert.Single(tips);
            Assert.Contains("Транзакцій не знайдено", tips[0]);
        }

        [Fact]
        public void ReturnsUpToThreeTips_WhenTransactionsExist()
        {
            // --- ЗМІНЕНО ТУТ ---
            var context = GetInMemoryDbContext();
            var service = new AdviceService(context);
            // --- КІНЕЦЬ ЗМІН ---

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 500, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-1), Category = new Category { Name = "Продукти" } },
                new Transaction { Amount = 300, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-2), Category = new Category { Name = "Підписки" } },
                new Transaction { Amount = 5, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-3), Category = new Category { Name = "Кава" } },
                new Transaction { Amount = 7, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-4), Category = new Category { Name = "Кава" } },
                new Transaction { Amount = 6, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-5), Category = new Category { Name = "Кава" } },
                new Transaction { Amount = 4, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-6), Category = new Category { Name = "Кава" } },
                new Transaction { Amount = 3, Type = TransactionType.Expense, Date = DateTime.Now.AddDays(-7), Category = new Category { Name = "Кава" } },
            };

            var tips = service.GetAdvice(transactions);

            Assert.InRange(tips.Count, 1, 3);
            Assert.Contains(tips[0], tips);
        }
    }
}
