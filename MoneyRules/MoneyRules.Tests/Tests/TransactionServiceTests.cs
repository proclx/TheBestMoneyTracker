using Xunit;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;

namespace MoneyRules.Tests.Services
{
    public class TransactionServiceTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            context.Transactions.Add(new Transaction
            {
                TransactionId = 1,
                UserId = 10,
                Amount = 100,
                Description = "Test transaction"
            });
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task DeleteTransactionAsync_ShouldReturnTrue_WhenTransactionExists()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var result = await service.DeleteTransactionAsync(1);

            Assert.True(result);
            Assert.Empty(context.Transactions);
        }

        [Fact]
        public async Task DeleteTransactionAsync_ShouldReturnFalse_WhenTransactionNotFound()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new TransactionService(context);

            var result = await service.DeleteTransactionAsync(999);

            Assert.False(result);
            Assert.Single(context.Transactions);
        }
    }
}
