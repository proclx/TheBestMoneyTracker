using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;
using MoneyRules.Infrastructure.Persistence;
using Xunit;

namespace MoneyRules.Tests
{
    public class TransactionTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly User _user;

        public TransactionTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDb();

            // Додаємо користувача для тестів
            _user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash"
            };
            _context.Users.Add(_user);
            _context.SaveChanges();
        }

        [Fact]
        public void AddTransaction_ShouldSaveTransactionWithCorrectValues()
        {
            // Arrange
            var amount = 150.50m;
            var selectedDate = new DateTime(2025, 10, 15);
            var utcDate = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
            var transactionType = TransactionType.Income;
            var categoryType = CategoryType.Category1;

            var transaction = new Transaction
            {
                UserId = _user.UserId,
                Type = transactionType,
                Category = new Category
                {
                    Name = "Food",
                    Type = categoryType,
                    UserId = _user.UserId
                },
                Amount = amount,
                Date = utcDate,
                Description = "Test transaction"
            };

            // Act
            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            var savedTransaction = _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefault();

            // Assert
            Assert.NotNull(savedTransaction);
            Assert.Equal(_user.UserId, savedTransaction.UserId);
            Assert.Equal(amount, savedTransaction.Amount);
            Assert.Equal(transactionType, savedTransaction.Type);
            Assert.Equal("Food", savedTransaction.Category.Name);
            Assert.Equal(categoryType, savedTransaction.Category.Type);
            Assert.Equal(DateTimeKind.Utc, savedTransaction.Date.Kind);
        }

        [Fact]
        public void AddTransaction_ShouldThrowFormatException_ForInvalidAmount()
        {
            // Arrange
            var invalidAmountText = "abc";

            // Act & Assert
            Assert.Throws<FormatException>(() =>
            {
                var amount = decimal.Parse(invalidAmountText);
            });
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
