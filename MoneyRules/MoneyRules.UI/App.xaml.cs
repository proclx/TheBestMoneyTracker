using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MoneyRules.Infrastructure.Persistence;
using MoneyRules.Application.Services;
using MoneyRules.UI.Windows;
using MoneyRules.Application.Interfaces;
using MoneyRules.UI.Utils; 
using System.Threading.Tasks;
using System;
using MoneyRules.Domain.Entities;

namespace MoneyRules.UI
{
    public partial class App : System.Windows.Application
    {
        public IServiceProvider? ServiceProvider { get; set; }
        public IConfiguration? Configuration { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
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
                services.AddScoped<IScheduledPaymentService, ScheduledPaymentService>();
                
                // =======================================================
                // ДОДАНО: Реєстрація сервісу сповіщень
                services.AddScoped<IPlannedPaymentNotificationService, PlannedPaymentNotificationService>();
                // =======================================================

                // Вікна
                services.AddTransient<WelcomeWindow>();
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<RegisterWindow>();
                services.AddTransient<AddTransactionWindow>();
                services.AddTransient<ScheduledPaymentsWindow>();
                
                // =======================================================
                // ВИДАЛЕНО: Реєстрація неіснуючої DataEditingPage
                // =======================================================


                ServiceProvider = services.BuildServiceProvider();
                Log.Debug("OnStartup: ServiceProvider створено.");

                //
                // =================================================================
                //  ВИПРАВЛЕННЯ: СПОЧАТКУ МІГРАЦІЯ, ПОТІМ ВХІД
                // =================================================================
                //

                // --- 1. ПЕРЕВІРКА МІГРАЦІЙ (ЗАПУСКАЄМО ПЕРЕД ВСІМ) ---
                using (var scope = ServiceProvider.CreateScope())
                {
                    Log.Debug("OnStartup: Застосування міграцій бази даних...");
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate(); 
                    Log.Debug("OnStartup: Міграції застосовано.");
                }

                // --- 2. ЛОГІКА АВТО-ВХОДУ (ЗАПУСКАЄМО ПІСЛЯ МІГРАЦІЙ) ---
                Log.Debug("OnStartup: Перевірка наявності токена...");
                string? token = SecureTokenStorage.LoadToken(); 
                User? user = null;

                if (!string.IsNullOrEmpty(token))
                {
                    Log.Debug("OnStartup: Токен знайдено, спроба входу...");
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


                if (user != null)
                {
                    Log.Information("OnStartup: Вхід за токеном успішний. Відкриття MainWindow.");
                    
                    // --- ДОДАНО ДЛЯ ТЕМИ ---
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var userProfileService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();
                        var settings = await userProfileService.GetUserSettingsAsync(user.UserId);
                        var theme = settings?.Theme ?? "Light";

                        if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
                        {
                            ThemeManager.SwitchTheme(Theme.Dark);
                        }
                        else
                        {
                            ThemeManager.SwitchTheme(Theme.Light);
                        }
                    }
                    // --- КІНЕЦЬ БЛОКУ ТЕМИ ---
                    
                    System.Windows.Application.Current.Properties["CurrentUser"] = user;
                    var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                }
                else
                {
                    Log.Information("OnStartup: Вхід за токеном НЕ вдався. Відкриття WelcomeWindow.");
                    
                    // --- ДОДАНО ДЛЯ ТЕМИ ---
                    ThemeManager.SwitchTheme(Theme.Light); // Встановлюємо світлу тему

                    var welcomeWindow = ServiceProvider.GetRequiredService<WelcomeWindow>();
                    welcomeWindow.Show();
                }
                // --- КІНЕЦЬ ЛОГІКИ АВТО-ВХОДУ ---

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критична помилка під час запуску програми");
                MessageBox.Show(ex.Message, "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.CloseAndFlush();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("--- Завершення роботи програми ---");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
        
        // Додаємо Helper-метод для отримання сервісів
        public static T GetService<T>() where T : class
        {
            return (System.Windows.Application.Current as App)?.ServiceProvider?.GetService(typeof(T)) as T;
        }
    }
}