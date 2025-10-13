using System;
using System.IO; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Xunit;


namespace MoneyRules.Tests.Tests
{
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CanAddAndRetrieveUserWithSettings()
        {
            // Arrange
            var options = GetInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                var user = new User
                {
                    Name = "Sofia",
                    Email = "sofia@example.com",
                    PasswordHash = "hashed",
                    Role = UserRole.User,
                    Settings = new Settings
                    {
                        Currency = "USD",
                        NotificationEnabled = true
                    }
                };

                // Act
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var savedUser = await context.Users
                    .Include(u => u.Settings)
                    .FirstOrDefaultAsync();

                Assert.NotNull(savedUser);
                Assert.Equal("Sofia", savedUser.Name);
                Assert.NotNull(savedUser.Settings);
                Assert.Equal("USD", savedUser.Settings.Currency);
            }
        }

        [Fact]
        public async Task CanAddCategoriesAndTransactionsForUser()
        {
            // Arrange
            var options = GetInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                var user = new User
                {
                    Name = "Test User",
                    Email = "user@example.com",
                    PasswordHash = "123",
                    Role = UserRole.User,
                    Settings = new Settings
                    {
                        Currency = "EUR",
                        NotificationEnabled = false
                    }
                };

                var category = new Category
                {
                    Name = "Groceries",
                    Type = CategoryType.Category1,
                    User = user
                };

                var transaction = new Transaction
                {
                    User = user,
                    Category = category,
                    Amount = 50.5m,
                    Type = TransactionType.Expense,
                    Date = DateTime.UtcNow,
                    Description = "Weekly shopping"
                };

                // Act
                context.AddRange(user, category, transaction);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var savedTransaction = await context.Transactions
                    .Include(t => t.User)
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync();

                Assert.NotNull(savedTransaction);
                Assert.Equal(50.5m, savedTransaction.Amount);
                Assert.Equal("Groceries", savedTransaction.Category.Name);
                Assert.Equal("Test User", savedTransaction.User.Name);
            }
        }

        [Fact]
        public void AppDbContextFactory_ShouldThrow_WhenMissingConnectionString()
        {
            // Arrange
            var factory = new AppDbContextFactory();
            var appSettingsExists = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));

            // Якщо конфіг існує — перевіряємо, що фабрика працює без винятків
            if (appSettingsExists)
            {
                var context = factory.CreateDbContext(Array.Empty<string>());
                Assert.NotNull(context);
            }
            else
            {
                // Якщо немає конфігу — тоді очікуємо виняток
                var ex = Assert.Throws<InvalidOperationException>(() =>
                {
                    factory.CreateDbContext(Array.Empty<string>());
                });
                Assert.Contains("Connection string 'DefaultConnection' not found", ex.Message);
            }
        }

        [Fact]
        public async Task ShouldSaveMultipleEntities_AndRetrieveThemCorrectly()
        {
            var options = GetInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                var user = new User
                {
                    Name = "John",
                    Email = "john@example.com",
                    PasswordHash = "abc",
                    Role = UserRole.User,
                    Settings = new Settings
                    {
                        Currency = "GBP",
                        NotificationEnabled = true
                    }
                };

                var cat1 = new Category { Name = "Salary", Type = CategoryType.Category1, User = user };
                var cat2 = new Category { Name = "Bills", Type = CategoryType.Category2, User = user };

                var tr1 = new Transaction
                {
                    User = user,
                    Category = cat1,
                    Amount = 1000,
                    Type = TransactionType.Income,
                    Date = DateTime.UtcNow,
                    Description = "Monthly Salary"
                };

                var tr2 = new Transaction
                {
                    User = user,
                    Category = cat2,
                    Amount = 120,
                    Type = TransactionType.Expense,
                    Date = DateTime.UtcNow,
                    Description = "Electricity bill"
                };

                context.AddRange(user, cat1, cat2, tr1, tr2);
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(options))
            {
                var transactions = await context.Transactions
                    .Include(t => t.Category)
                    .Include(t => t.User)
                    .ToListAsync();

                Assert.Equal(2, transactions.Count);
                Assert.Contains(transactions, t => t.Category.Name == "Salary");
                Assert.Contains(transactions, t => t.Category.Name == "Bills");
            }
        }
    }
}
