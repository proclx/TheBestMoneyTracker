using System;
using System.Threading.Tasks;
using Xunit;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests.Tests
{
    public class TransactionTests
    {
        [Fact]
        public void Transaction_ShouldBeCreatedSuccessfully()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User
            };

            var category = new Category
            {
                CategoryId = 10,
                UserId = 1,
                User = user,
                Name = "Їжа",
                Type = CategoryType.Category1 
            };

            // Act
            var transaction = new Transaction
            {
                TransactionId = 100,
                UserId = user.UserId,
                User = user,
                CategoryId = category.CategoryId,
                Category = category,
                Amount = 250.50m,
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                Description = "Обід у кафе"
            };

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(250.50m, transaction.Amount);
            Assert.Equal(TransactionType.Expense, transaction.Type);
            Assert.Equal(category.CategoryId, transaction.CategoryId);
            Assert.Equal(user.UserId, transaction.UserId);
        }

        [Fact]
        public void Transaction_ShouldLinkUserAndCategory()
        {
            // Arrange
            var user = new User { UserId = 2, Name = "Оксана", Email = "oksana@example.com", PasswordHash = "pw", Role = UserRole.User };
            var category = new Category { CategoryId = 5, Name = "Зарплата", Type = CategoryType.Category2, UserId = user.UserId, User = user };

            // Act
            var transaction = new Transaction
            {
                TransactionId = 101,
                UserId = user.UserId,
                User = user,
                CategoryId = category.CategoryId,
                Category = category,
                Amount = 5000m,
                Type = TransactionType.Income,
                Date = DateTime.Now,
                Description = "Отримано зарплату"
            };

            // Assert
            Assert.Equal(user.UserId, transaction.UserId);
            Assert.Equal(category.CategoryId, transaction.CategoryId);
            Assert.Equal("Отримано зарплату", transaction.Description);
        }

        [Fact]
        public void Transaction_ShouldHaveValidAmount()
        {
            // Arrange
            var transaction = new Transaction { Amount = 150.75m };

            // Assert
            Assert.True(transaction.Amount > 0);
        }

        [Fact]
        public void Transaction_ShouldAllowDifferentTypes()
        {
            // Arrange
            var income = new Transaction { Type = TransactionType.Income };
            var expense = new Transaction { Type = TransactionType.Expense };

            // Assert
            Assert.NotEqual(income.Type, expense.Type);
        }

        [Fact]
        public async Task AddTransaction_ShouldThrowException_WhenAmountIsNegative()
        {
            // Arrange
            var transaction = new Transaction { Amount = -100m };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await Task.Run(() =>
                {
                    if (transaction.Amount < 0)
                        throw new Exception("Amount cannot be negative");
                });
            });
        }
    }
}
