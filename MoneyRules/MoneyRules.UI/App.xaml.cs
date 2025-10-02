using System;
using System.Configuration;
using System.Data;
using System.Windows;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MoneyRules.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Логер Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Додаток стартує");
            try
            {
                // Код, який може викликати помилку
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Сталася помилка під час виконання операції");
            }

            base.OnStartup(e);

            // Зчитуємо рядок підключення з App.config
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Створюємо екземпляр контексту
            using var context = new AppDbContext(optionsBuilder.Options);

            // Можна зробити EnsureCreated для тесту
            context.Database.EnsureCreated();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

    }

}
