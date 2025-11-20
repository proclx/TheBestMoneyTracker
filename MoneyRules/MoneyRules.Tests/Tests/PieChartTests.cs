using System;
using System.Linq;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using Xunit;

namespace MoneyRules.Tests
{
    public class PieChartTests
    {
        [Fact]
        public void GetCategoryTotals_ReturnsExpenseTotalsGroupedByCategory()
        {
            var ctx = TestDbContextFactory.CreateInMemoryDb();

            var user = new User { UserId = 1, Name = "Test", Email = "t@test.local" };
            ctx.Users.Add(user);

            var catFood = new Category { CategoryId = 1, UserId = 1, Name = "Food", Type = CategoryType.Category2 };
            var catTrans = new Category { CategoryId = 2, UserId = 1, Name = "Transport", Type = CategoryType.Category2 };
            ctx.Categories.AddRange(catFood, catTrans);

            ctx.Transactions.AddRange(
                new Transaction { TransactionId = 1, UserId = 1, Category = catFood, CategoryId = 1, Amount = 10m, Type = TransactionType.Expense, Date = new DateTime(2025, 1, 10), Description = "Lunch" },
                new Transaction { TransactionId = 2, UserId = 1, Category = catFood, CategoryId = 1, Amount = 5m, Type = TransactionType.Expense, Date = new DateTime(2025, 1, 11), Description = "Coffee" },
                new Transaction { TransactionId = 3, UserId = 1, Category = catTrans, CategoryId = 2, Amount = 7m, Type = TransactionType.Expense, Date = new DateTime(2025, 1, 12), Description = "Taxi" },
                new Transaction { TransactionId = 4, UserId = 1, Category = catFood, CategoryId = 1, Amount = 100m, Type = TransactionType.Income, Date = new DateTime(2025, 1, 15), Description = "Salary" }
            );

            ctx.SaveChanges();

            var svc = new ChartService(ctx);
            var totals = svc.GetCategoryTotals(1);

            Assert.NotNull(totals);
            Assert.Equal(2, totals.Count);
            Assert.True(totals.ContainsKey("Food"));
            Assert.True(totals.ContainsKey("Transport"));
            Assert.Equal(15m, totals["Food"]);
            Assert.Equal(7m, totals["Transport"]);
        }

        [Fact]
        public void GetCategoryTotals_FiltersByYearAndMonth()
        {
            var ctx = TestDbContextFactory.CreateInMemoryDb();

            var user = new User { UserId = 2, Name = "FilterUser", Email = "f@test.local" };
            ctx.Users.Add(user);

            var cat = new Category { CategoryId = 10, UserId = 2, Name = "Groceries", Type = CategoryType.Category2 };
            ctx.Categories.Add(cat);

            // Transactions in different months and years
            ctx.Transactions.AddRange(
                new Transaction { TransactionId = 10, UserId = 2, Category = cat, CategoryId = 10, Amount = 20m, Type = TransactionType.Expense, Date = new DateTime(2024, 12, 5) },
                new Transaction { TransactionId = 11, UserId = 2, Category = cat, CategoryId = 10, Amount = 30m, Type = TransactionType.Expense, Date = new DateTime(2025, 1, 6) },
                new Transaction { TransactionId = 12, UserId = 2, Category = cat, CategoryId = 10, Amount = 50m, Type = TransactionType.Expense, Date = new DateTime(2025, 2, 7) }
            );

            ctx.SaveChanges();

            var svc = new ChartService(ctx);

            var totalsJan2025 = svc.GetCategoryTotals(2, 2025, 1);
            Assert.Single(totalsJan2025);
            Assert.Equal(30m, totalsJan2025["Groceries"]);

            var totals2025 = svc.GetCategoryTotals(2, 2025, null);
            Assert.Single(totals2025);
            Assert.Equal(80m, totals2025["Groceries"]);

            var totalsAll = svc.GetCategoryTotals(2, null, null);
            Assert.Single(totalsAll);
            Assert.Equal(100m, totalsAll["Groceries"]);
        }

        [Fact]
public void GetCategoryTotals_HandlesTransactionsWithoutCategory()
{
    var ctx = TestDbContextFactory.CreateInMemoryDb();

    var user = new User { UserId = 4, Name = "UncategorizedUser", Email = "u@test.local" };
    ctx.Users.Add(user);
    
    // Транзакція 1: Без Category, але з Description
    ctx.Transactions.Add(
        new Transaction { 
            TransactionId = 30, 
            UserId = 4, 
            CategoryId = 0,
            Amount = 12.50m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2025, 5, 1), 
            Description = "Random Shop Purchase" 
        }
    );
    
    // Транзакція 2: Без Category і без Description (опис буде "(Без категорії)")
    ctx.Transactions.Add(
        new Transaction { 
            TransactionId = 31, 
            UserId = 4, 
            CategoryId = 0, 
            Amount = 5.00m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2025, 5, 2), 
            Description = null 
        }
    );

    ctx.SaveChanges();

    var svc = new ChartService(ctx);
    var totals = svc.GetCategoryTotals(4);

    Assert.NotNull(totals);
    Assert.Equal(2, totals.Count); // Очікуємо 2 записи: "Random Shop Purchase" та "(Без категорії)"
    
    // Перевірка транзакції з описом, але без категорії
    Assert.True(totals.ContainsKey("Random Shop Purchase"));
    Assert.Equal(12.50m, totals["Random Shop Purchase"]);
    
    // Перевірка транзакції без категорії та без опису
    Assert.True(totals.ContainsKey("(Без категорії)"));
    Assert.Equal(5.00m, totals["(Без категорії)"]);
}

        [Fact]
        public void GetMonthlyStatistics_SumsIncomeAndExpensePerMonth()
        {
            var ctx = TestDbContextFactory.CreateInMemoryDb();

            var user = new User { UserId = 3, Name = "MonthlyUser", Email = "m@test.local" };
            ctx.Users.Add(user);

            var cat = new Category { CategoryId = 20, UserId = 3, Name = "Misc", Type = CategoryType.Category2 };
            ctx.Categories.Add(cat);

            ctx.Transactions.AddRange(
                new Transaction { TransactionId = 20, UserId = 3, Category = cat, CategoryId = 20, Amount = 100m, Type = TransactionType.Income, Date = new DateTime(2025, 3, 1) },
                new Transaction { TransactionId = 21, UserId = 3, Category = cat, CategoryId = 20, Amount = 40m, Type = TransactionType.Expense, Date = new DateTime(2025, 3, 2) },
                new Transaction { TransactionId = 22, UserId = 3, Category = cat, CategoryId = 20, Amount = 60m, Type = TransactionType.Expense, Date = new DateTime(2025, 4, 3) }
            );

            ctx.SaveChanges();

            var svc = new ChartService(ctx);
            var monthly = svc.GetMonthlyStatistics(3, 2025);

            // March -> income 100, expense 40
            Assert.Equal(100m, monthly[3].Income);
            Assert.Equal(40m, monthly[3].Expense);

            // April -> expense 60
            Assert.Equal(60m, monthly[4].Expense);
            Assert.Equal(0m, monthly[4].Income);
        }
    }
}
