using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoneyRules.Infrastructure.Persistence;
using System.IO;

namespace MoneyRules.UI.Utils
{
    public static class DbContextFactory
    {
        public static AppDbContext Create()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) 
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // ВАЖЛИВО: Переконайтеся, що назва "DefaultConnection" 
            // у вашому appsettings.json
            var connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");
            }

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            
            // Цей конструктор (з options) має існувати у AppDbContext
            return new AppDbContext(options);
        }
    }
}