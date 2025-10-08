using System;
using System.Data;
using System.IO;
using System.Windows;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MoneyRules.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 1️⃣ Налаштування Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Додаток стартує");

            try
            {
                // 2️⃣ Зчитуємо рядок підключення з appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' не знайдено в appsettings.json.");

                // 3️⃣ Створюємо DbContext
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                using var context = new AppDbContext(optionsBuilder.Options);

                // 4️⃣ Для тесту — створюємо базу, якщо її ще немає
                context.Database.EnsureCreated();

                Log.Information("Підключення до бази даних успішне!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Сталася помилка під час запуску програми");
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
