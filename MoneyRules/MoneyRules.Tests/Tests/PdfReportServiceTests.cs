using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using Xunit;

namespace MoneyRules.Tests.Tests
{
    public class PdfReportServiceTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void CreateTransactionsReport_CreatesFile_WithIncomeAndExpenseLabels()
        {
            var options = GetInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                var user = new User { Name = "U", Email = "u@example.com" };
                context.Users.Add(user);

                var cat = new Category { Name = "Salary", User = user };
                var t1 = new Transaction { User = user, Category = cat, Amount = 1000m, Type = TransactionType.Income, Date = DateTime.UtcNow, Description = "Salary" };
                var t2 = new Transaction { User = user, Category = cat, Amount = 50m, Type = TransactionType.Expense, Date = DateTime.UtcNow, Description = "Coffee" };

                context.AddRange(cat, t1, t2);
                context.SaveChanges();
            }

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
            try
            {
                // factory to return context with same options
                Func<AppDbContext> factory = () => new AppDbContext(options);
                var svc = new PdfReportService(factory);
                svc.CreateTransactionsReport(tempPath, userId: 1);

                Assert.True(File.Exists(tempPath));
                var bytes = File.ReadAllBytes(tempPath);
                Assert.True(bytes.Length > 100); // simple sanity

                // quick binary search for Ukrainian words (UTF-8 in PDF streams may exist)
                var content = System.Text.Encoding.UTF8.GetString(bytes.Take(2000).ToArray());
                Assert.Contains("Дохід", content);
                Assert.Contains("Витрати", content);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public void CreateTransactionsReport_UserFilter_ReducesContent()
        {
            var options = GetInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                var u1 = new User { Name = "A", Email = "a@example.com" };
                var u2 = new User { Name = "B", Email = "b@example.com" };
                context.Users.AddRange(u1, u2);

                var cat1 = new Category { Name = "Food", User = u1 };
                var cat2 = new Category { Name = "Salary", User = u2 };

                var t1 = new Transaction { User = u1, Category = cat1, Amount = 10m, Type = TransactionType.Expense, Date = DateTime.UtcNow, Description = "Snack" };
                var t2 = new Transaction { User = u2, Category = cat2, Amount = 2000m, Type = TransactionType.Income, Date = DateTime.UtcNow, Description = "Salary" };

                context.AddRange(cat1, cat2, t1, t2);
                context.SaveChanges();
            }

            var tempAll = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_all.pdf");
            var tempUser1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_u1.pdf");
            try
            {
                Func<AppDbContext> factory = () => new AppDbContext(options);
                var svc = new PdfReportService(factory);
                svc.CreateTransactionsReport(tempAll, userId: null);
                svc.CreateTransactionsReport(tempUser1, userId: 1);

                var allBytes = File.ReadAllBytes(tempAll);
                var u1Bytes = File.ReadAllBytes(tempUser1);

                Assert.True(allBytes.Length >= u1Bytes.Length);
                Assert.True(u1Bytes.Length > 0);
            }
            finally
            {
                if (File.Exists(tempAll)) File.Delete(tempAll);
                if (File.Exists(tempUser1)) File.Delete(tempUser1);
            }
        }
    }
}
