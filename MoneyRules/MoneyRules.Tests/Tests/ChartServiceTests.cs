using System;
using System.Linq;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Xunit;

namespace MoneyRules.Tests.Tests
{
    public class ChartServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly User _user;

        public ChartServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDb();

            _user = new User
            {
                UserId = 1,
                Name = "Chart User",
                Email = "chart@example.com",
                PasswordHash = "hash"
            };

            _context.Users.Add(_user);
            _context.SaveChanges();
        }

        [Fact]
        public void GetDailyStatistics_ReturnsAllDaysAndSums()
        {
            var year = 2025;
            var month = 10;

            // Day 3: income 200
            _context.Transactions.Add(new Transaction
            {
                UserId = _user.UserId,
                Type = TransactionType.Income,
                Amount = 200m,
                Date = DateTime.SpecifyKind(new DateTime(year, month, 3), DateTimeKind.Utc),
                Category = new Category { Name = "Salary", Type = CategoryType.Category1, UserId = _user.UserId }
            });

            // Day 10: expense 50
            _context.Transactions.Add(new Transaction
            {
                UserId = _user.UserId,
                Type = TransactionType.Expense,
                Amount = 50m,
                Date = DateTime.SpecifyKind(new DateTime(year, month, 10), DateTimeKind.Utc),
                Category = new Category { Name = "Food", Type = CategoryType.Category1, UserId = _user.UserId }
            });

            _context.SaveChanges();

            var service = new ChartService(_context);
            var daily = service.GetDailyStatistics(_user.UserId, year, month);

            var daysInMonth = DateTime.DaysInMonth(year, month);
            Assert.Equal(daysInMonth, daily.Count);

            Assert.Equal(200m, daily[3].Income);
            Assert.Equal(0m, daily[3].Expense);

            Assert.Equal(0m, daily[10].Income);
            Assert.Equal(50m, daily[10].Expense);
        }

        [Fact]
        public void ChartViewHelper_FiltersOnlyNonZeroDays()
        {
            var year = 2025;
            var month = 11;

            // Day 5 income and day 20 expense
            _context.Transactions.Add(new Transaction
            {
                UserId = _user.UserId,
                Type = TransactionType.Income,
                Amount = 120m,
                Date = DateTime.SpecifyKind(new DateTime(year, month, 5), DateTimeKind.Utc),
                Category = new Category { Name = "Gift", Type = CategoryType.Category1, UserId = _user.UserId }
            });

            _context.Transactions.Add(new Transaction
            {
                UserId = _user.UserId,
                Type = TransactionType.Expense,
                Amount = 30m,
                Date = DateTime.SpecifyKind(new DateTime(year, month, 20), DateTimeKind.Utc),
                Category = new Category { Name = "Taxi", Type = CategoryType.Category1, UserId = _user.UserId }
            });

            _context.SaveChanges();

            var service = new ChartService(_context);
            var daily = service.GetDailyStatistics(_user.UserId, year, month);

            var filtered = daily
                .Where(kvp => kvp.Value.Income != 0m || kvp.Value.Expense != 0m)
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => (Day: kvp.Key, Income: kvp.Value.Income, Expense: kvp.Value.Expense))
                .ToList();

            Assert.Equal(2, filtered.Count);
            Assert.Equal(5, filtered[0].Day);
            Assert.Equal(120m, filtered[0].Income);
            Assert.Equal(20, filtered[1].Day);
            Assert.Equal(30m, filtered[1].Expense);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
