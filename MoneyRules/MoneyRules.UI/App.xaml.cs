using System;
using System.Configuration;
using System.Data;
using System.Windows;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MoneyRules.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
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

    }

}
