using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;

namespace MoneyRules.UI
{
    public partial class App : System.Windows.Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var services = new ServiceCollection();

                // DbContext
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

                // Сервіси
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<ITransactionService, TransactionService>();

                // Вікна
                services.AddTransient<WelcomeWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<RegisterWindow>();
                services.AddTransient<AddTransactionWindow>();

                ServiceProvider = services.BuildServiceProvider();

                // Перевіримо БД
                using (var scope = ServiceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }

                var welcomeWindow = ServiceProvider.GetRequiredService<WelcomeWindow>();
                welcomeWindow.Show();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Помилка під час запуску програми");
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }

    }
}

