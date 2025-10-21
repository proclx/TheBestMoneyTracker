using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace MoneyRules.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public AppDbContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>()
                .HasKey(s => s.UserId); // PK = FK

            modelBuilder.Entity<User>()
                .HasOne(u => u.Settings)
                .WithOne(s => s.User)
                .HasForeignKey<Settings>(s => s.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }
}
