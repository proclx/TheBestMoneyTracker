using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoneyRules.Domain.Entities;

namespace MoneyRules.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        // 🟩 Таблиці (DbSet-и)
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Settings> Settings { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ExpenseLimit> ExpenseLimits { get; set; } = null!; // ✅ додано

        // Конструктори
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public AppDbContext() { }

        // Налаштування зв’язків між сутностями
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Settings — 1:1 з User
            modelBuilder.Entity<Settings>()
                .HasKey(s => s.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Settings)
                .WithOne(s => s.User)
                .HasForeignKey<Settings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transactions — 1:N з User
            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Categories — 1:N з User
            modelBuilder.Entity<User>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Transactions — 1:N з Category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // ExpenseLimit — 1:N з User (один користувач має багато лімітів)
            modelBuilder.Entity<ExpenseLimit>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ExpenseLimit>()
                .HasIndex(e => new { e.UserId, e.Year, e.Month })
                .IsUnique(); // кожен користувач має лише один ліміт на місяць

            base.OnModelCreating(modelBuilder);
        }

        // Конфігурація з appsettings.json
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var basePath = Directory.GetCurrentDirectory();
                var configPath = Path.Combine(basePath, "appsettings.json");

                if (!File.Exists(configPath))
                    throw new FileNotFoundException($"Файл конфігурації не знайдено: {configPath}");

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' не знайдено в appsettings.json.");

                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
}
