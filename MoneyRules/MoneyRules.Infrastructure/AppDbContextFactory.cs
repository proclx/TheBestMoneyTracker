using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MoneyRules.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Зчитуємо конфігурацію з App.config або appsettings.json
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Тут вказуємо свій рядок підключення
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=MoneyRules;Username=postgres;Password=123");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

