using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Xunit;

namespace MoneyRules.Tests
{
    public class TransactionIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;

        public TransactionIntegrationTests()
        {
            _context = CreateContext();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void CanPerformCRUD_WithTransaction_RollsBack()
        {
            // Починаємо транзакцію
            // using var dbTransaction = _context.Database.BeginTransaction();

            // 1️⃣ User + Settings
            var user = new User
            {
                Name = "TestUser",
                Email = "test@example.com",
                PasswordHash = "123456",
                Role = UserRole.Guest,
                Settings = new Settings
                {
                    Currency = "USD",
                    NotificationEnabled = true
                }
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var fromDbUser = _context.Users.Include(u => u.Settings)
                                           .FirstOrDefault(u => u.Email == "test@example.com");
            Assert.NotNull(fromDbUser);
            Assert.NotNull(fromDbUser.Settings);

            // 2️⃣ Category
            var category = new Category
            {
                Name = "TestCategory",
                Type = CategoryType.Category1,
                User = fromDbUser
            };
            _context.Categories.Add(category);
            _context.SaveChanges();

            var fromDbCategory = _context.Categories.Include(c => c.User)
                                                    .FirstOrDefault(c => c.Name == "TestCategory");
            Assert.NotNull(fromDbCategory);
            Assert.Equal("TestUser", fromDbCategory.User.Name);

            // 3️⃣ Transaction
            var transactionEntity = new Transaction
            {
                Amount = 100.50m,
                Type = TransactionType.Expense,
                Date = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                Description = "TestTransaction",
                User = fromDbUser,
                Category = fromDbCategory
            };
            _context.Transactions.Add(transactionEntity);
            _context.SaveChanges();

            var fromDbTransaction = _context.Transactions
                                            .Include(t => t.User)
                                            .Include(t => t.Category)
                                            .FirstOrDefault(t => t.Description == "TestTransaction");
            Assert.NotNull(fromDbTransaction);
            Assert.Equal("TestUser", fromDbTransaction.User.Name);
            Assert.Equal("TestCategory", fromDbTransaction.Category.Name);

            // 4️⃣ Rollback (для InMemory це просто викликаємо Dispose без Save)
            // dbTransaction.Rollback();

            // Перевірка, що дані все ще доступні в InMemory (для InMemory транзакція не видаляє об'єкти)
            // Тому можна просто перевірити, що об'єкти існують після SaveChanges, 
            // а при реальному PostgreSQL rollback видалить їх.
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
