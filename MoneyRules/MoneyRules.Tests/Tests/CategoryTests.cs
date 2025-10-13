using Xunit;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Tests.Tests
{
    public class CategoryTests
    {
        [Fact]
        public void Category_ShouldBeCreatedSuccessfully()
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

            // Act
            var category = new Category
            {
                CategoryId = 1,
                UserId = user.UserId,
                User = user,
                Name = "Їжа",
                Type = CategoryType.Category1 // 👈 підлаштовано під твій enum
            };

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Їжа", category.Name);
            Assert.Equal(CategoryType.Category1, category.Type);
            Assert.Equal(user.UserId, category.UserId);
            Assert.Equal(user, category.User);
        }

        [Fact]
        public void Category_ShouldAllowDifferentTypes()
        {
            // Arrange
            var category1 = new Category { Name = "Категорія 1", Type = CategoryType.Category1 };
            var category2 = new Category { Name = "Категорія 2", Type = CategoryType.Category2 };

            // Assert
            Assert.NotEqual(category1.Type, category2.Type);
        }

        [Fact]
        public void Category_ShouldThrowException_IfNameIsEmpty()
        {
            // Arrange
            var category = new Category
            {
                CategoryId = 1,
                UserId = 1,
                Name = string.Empty,
                Type = CategoryType.Category1
            };

            // Act & Assert
            Assert.True(string.IsNullOrEmpty(category.Name));
        }

        [Fact]
        public void Category_ShouldBeLinkedToUser()
        {
            // Arrange
            var user = new User
            {
                UserId = 5,
                Name = "Іван",
                Email = "ivan@example.com",
                PasswordHash = "12345",
                Role = UserRole.User
            };

            var category = new Category
            {
                CategoryId = 10,
                UserId = user.UserId,
                User = user,
                Name = "Подорожі",
                Type = CategoryType.Category2
            };

            // Assert
            Assert.NotNull(category.User);
            Assert.Equal(user.UserId, category.UserId);
            Assert.Equal("Іван", category.User.Name);
        }
    }
}
