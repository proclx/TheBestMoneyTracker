using Xunit;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MoneyRules.Tests.Integration
{
    public class AppDbContextTransactionTests
    {
        private AppDbContext CreateContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options);
        }


        [Fact]
        public void CanPerformCRUD_WithTransaction_RollsBack()
        {
            using var context = CreateContext();

            // Починаємо транзакцію
            using var transaction = context.Database.BeginTransaction();

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
            context.Users.Add(user);
            context.SaveChanges();

            var fromDbUser = context.Users.Include(u => u.Settings)
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
            context.Categories.Add(category);
            context.SaveChanges();

            var fromDbCategory = context.Categories.Include(c => c.User)
                                                   .FirstOrDefault(c => c.Name == "TestCategory");
            Assert.NotNull(fromDbCategory);
            Assert.Equal("TestUser", fromDbCategory.User.Name);

            // 3️⃣ Transaction
            var transactionEntity = new Transaction
            {
                Amount = 100.50m,
                Type = TransactionType.Expense,
                Date = DateTime.UtcNow,
                Description = "TestTransaction",
                User = fromDbUser,
                Category = fromDbCategory
            };
            context.Transactions.Add(transactionEntity);
            context.SaveChanges();

            var fromDbTransaction = context.Transactions
                                           .Include(t => t.User)
                                           .Include(t => t.Category)
                                           .FirstOrDefault(t => t.Description == "TestTransaction");
            Assert.NotNull(fromDbTransaction);
            Assert.Equal("TestUser", fromDbTransaction.User.Name);
            Assert.Equal("TestCategory", fromDbTransaction.Category.Name);

            // 4️⃣ Rollback для відкату всіх змін
            transaction.Rollback();

            // Перевірка, що дані видалені
            var checkUser = context.Users.FirstOrDefault(u => u.Email == "test@example.com");
            Assert.Null(checkUser);
        }
    }
}
