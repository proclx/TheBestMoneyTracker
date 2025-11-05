using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog; // <-- Потрібно для логування
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.Utils; // <-- Потрібно для SecureTokenStorage
using System.Threading.Tasks; // <-- Потрібно для async
using System;
using MoneyRules.Domain.Entities; // <-- Потрібно для User

namespace MoneyRules.UI
{
    public partial class App : System.Windows.Application
    {
        public IServiceProvider? ServiceProvider { get; set; }
        public IConfiguration? Configuration { get; set; }

        // Змінено на async
        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Встановлюємо рівень Debug, щоб бачити все
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("--- Запуск програми ---");

            try
            {
                Log.Debug("OnStartup: Налаштування конфігурації...");
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                Log.Debug("OnStartup: Налаштування сервісів (DI)...");
                var services = new ServiceCollection();

                // DbContext
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

                // Сервіси
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<ITransactionService, TransactionService>();
                services.AddScoped<IUserProfileService, UserProfileService>();
                services.AddScoped<IAdviceService, AdviceService>();
                services.AddScoped<IChartService, ChartService>();
                services.AddScoped<ICurrencyService, CurrencyService>();
                services.AddScoped<IFileUploadService, FileUploadService>();

                // Вікна
                services.AddTransient<WelcomeWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<RegisterWindow>();
                services.AddTransient<AddTransactionWindow>();

                ServiceProvider = services.BuildServiceProvider();
                Log.Debug("OnStartup: ServiceProvider створено.");

                // --- ЛОГІКА АВТО-ВХОДУ ---

                Log.Debug("OnStartup: Перевірка наявності токена...");
                string? token = SecureTokenStorage.LoadToken(); // 'LoadToken' вже логує свій результат
                User? user = null;

                if (!string.IsNullOrEmpty(token))
                {
                    Log.Debug("OnStartup: Токен знайдено, спроба входу...");
                    // Потрібно отримати IAuthService з нашого нового ServiceProvider
                    // Використовуємо CreateScope для "одноразового" сервісу
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                        user = await authService.LoginWithTokenAsync(token);
                    }
                }
                else
                {
                    Log.Debug("OnStartup: Збережений токен не знайдено.");
                }

                // Перевірка міграцій
                using (var scope = ServiceProvider.CreateScope())
                {
                    Log.Debug("OnStartup: Застосування міграцій бази даних (EnsureCreated)...");
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated(); // Або db.Database.Migrate()
                }

                if (user != null)
                {
                    Log.Information("OnStartup: Вхід за токеном успішний. Відкриття MainWindow.");
                    // Зберігаємо користувача в сесії
                    System.Windows.Application.Current.Properties["CurrentUser"] = user;
                    // Відкриваємо головне вікно
                    var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                }
                else
                {
                    Log.Information("OnStartup: Вхід за токеном НЕ вдався. Відкриття WelcomeWindow.");
                    // Немає токена або він недійсний, показуємо вікно логіну
                    var welcomeWindow = ServiceProvider.GetRequiredService<WelcomeWindow>();
                    welcomeWindow.Show();
                }
                // --- КІНЕЦЬ ЛОГІКИ АВТО-ВХОДУ ---

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критична помилка під час запуску програми");
                MessageBox.Show(ex.Message, "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Важливо закрити логер, якщо сталася фатальна помилка
                Log.CloseAndFlush();
            }

            base.OnStartup(e);
        }

        // --- ДОДАНО НОВИЙ МЕТОД ---
        protected override void OnExit(ExitEventArgs e)
        {
            // Це ГАРАНТУЄ, що всі логи з буфера будуть збережені у файл
            Log.Information("--- Завершення роботи програми ---");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}

