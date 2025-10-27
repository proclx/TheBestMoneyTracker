using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;

namespace MoneyRules.Tests.Tests
{
    public class ChartServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void GetMonthlyTotals_AggregatesIncomeAndExpense()
        {
            using var db = CreateInMemoryContext();

            var user = new User { UserId = 1, Name = "T" };
            db.Users.Add(user);
            db.SaveChanges();

            // Add transactions: Jan income 100, Jan expense 30, Feb income 50
            db.Transactions.Add(new Transaction { TransactionId = 1, UserId = 1, Amount = 100m, Date = new DateTime(2025, 1, 5), Type = Domain.Enums.TransactionType.Income });
            db.Transactions.Add(new Transaction { TransactionId = 2, UserId = 1, Amount = 30m, Date = new DateTime(2025, 1, 10), Type = Domain.Enums.TransactionType.Expense });
            db.Transactions.Add(new Transaction { TransactionId = 3, UserId = 1, Amount = 50m, Date = new DateTime(2025, 2, 3), Type = Domain.Enums.TransactionType.Income });
            db.SaveChanges();

            var svc = new ChartService();
            var months = svc.GetMonthlyTotals(db, 1, 2025);

            var jan = months.First(m => m.Month == 1);
            var feb = months.First(m => m.Month == 2);
            var mar = months.First(m => m.Month == 3);

            Assert.Equal(100m, jan.Income);
            Assert.Equal(30m, jan.Expense);
            Assert.Equal(50m, feb.Income);
            Assert.Equal(0m, feb.Expense);
            Assert.Equal(0m, mar.Income);
            Assert.Equal(0m, mar.Expense);
        }
    }
}
