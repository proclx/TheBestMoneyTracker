using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Application.Services;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Xunit;

namespace MoneyRules.Tests.Tests
{
    public class ExpenseLimitServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // унікальна БД для кожного тесту
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task SetLimitAsync_Should_Add_New_Limit_When_None_Exists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new ExpenseLimitService(context);
            var userId = Guid.NewGuid();

            // Act
            await service.SetLimitAsync(userId, 1000m, 2025, 10);

            // Assert
            var limit = await context.ExpenseLimits
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Month == 10 && e.Year == 2025);

            Assert.NotNull(limit);
            Assert.Equal(1000m, limit.Amount);
        }

        [Fact]
        public async Task SetLimitAsync_Should_Update_Existing_Limit()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userId = Guid.NewGuid();
            var existing = new ExpenseLimit(userId, 500m, 2025, 10);
            await context.ExpenseLimits.AddAsync(existing);
            await context.SaveChangesAsync();

            var service = new ExpenseLimitService(context);

            // Act
            await service.SetLimitAsync(userId, 1500m, 2025, 10);

            // Assert
            var updated = await context.ExpenseLimits
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Month == 10 && e.Year == 2025);

            Assert.NotNull(updated);
            Assert.Equal(1500m, updated.Amount);
        }

        [Fact]
        public async Task GetLimitAsync_Should_Return_Correct_Limit()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userId = Guid.NewGuid();
            var limit = new ExpenseLimit(userId, 800m, 2025, 9);
            await context.ExpenseLimits.AddAsync(limit);
            await context.SaveChangesAsync();

            var service = new ExpenseLimitService(context);

            // Act
            var result = await service.GetLimitAsync(userId, 2025, 9);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(800m, result.Amount);
        }

        [Fact]
        public async Task GetAllLimitsAsync_Should_Return_All_Ordered()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userId = Guid.NewGuid();

            await context.ExpenseLimits.AddRangeAsync(
                new ExpenseLimit(userId, 100m, 2024, 1),
                new ExpenseLimit(userId, 200m, 2025, 5),
                new ExpenseLimit(userId, 300m, 2023, 12)
            );
            await context.SaveChangesAsync();

            var service = new ExpenseLimitService(context);

            // Act
            var result = await service.GetAllLimitsAsync(userId);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(result,
                first => Assert.Equal(2025, first.Year),
                second => Assert.Equal(2024, second.Year),
                third => Assert.Equal(2023, third.Year)
            );
        }

        [Fact]
        public async Task DeleteLimitAsync_Should_Remove_Existing_Limit()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userId = Guid.NewGuid();
            var limit = new ExpenseLimit(userId, 400m, 2025, 11);
            await context.ExpenseLimits.AddAsync(limit);
            await context.SaveChangesAsync();

            var service = new ExpenseLimitService(context);

            // Act
            await service.DeleteLimitAsync(limit.Id); // ✅ використовуємо Id
            var deleted = await context.ExpenseLimits.FindAsync(limit.Id);

            // Assert
            Assert.Null(deleted);
        }

        [Fact]
        public async Task SetLimitAsync_Should_Throw_When_Amount_Is_Invalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new ExpenseLimitService(context);
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.SetLimitAsync(userId, 0, 2025, 10));
        }

        [Fact]
        public async Task SetLimitAsync_Should_Throw_When_Month_Invalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new ExpenseLimitService(context);
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.SetLimitAsync(userId, 1000, 2025, 13));
        }
    }
}
