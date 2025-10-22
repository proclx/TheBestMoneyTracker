using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;

namespace MoneyRules.Application.Services
{
    /// <summary>
    /// Сервіс для роботи з лімітами витрат користувача.
    /// </summary>
    public class ExpenseLimitService
    {
        private readonly AppDbContext _context;

        public ExpenseLimitService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Встановлює або оновлює ліміт витрат для користувача на конкретний місяць і рік.
        /// </summary>
        public async Task SetLimitAsync(Guid userId, decimal amount, int year, int month)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума ліміту повинна бути більшою за 0.");

            if (month < 1 || month > 12)
                throw new ArgumentException("Місяць має бути в межах від 1 до 12.");

            if (year < 2000 || year > DateTime.Now.Year + 1)
                throw new ArgumentException("Невірно вказаний рік.");

            var existingLimit = await _context.ExpenseLimits
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year && e.Month == month);

            if (existingLimit != null)
            {
                existingLimit.Amount = amount;
                _context.ExpenseLimits.Update(existingLimit);
            }
            else
            {
                var newLimit = new ExpenseLimit(userId, amount, year, month);
                await _context.ExpenseLimits.AddAsync(newLimit);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Отримати ліміт для певного користувача за конкретний місяць і рік.
        /// </summary>
        public async Task<ExpenseLimit?> GetLimitAsync(Guid userId, int year, int month)
        {
            return await _context.ExpenseLimits
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year && e.Month == month);
        }

        /// <summary>
        /// Отримати всі ліміти користувача, відсортовані за датою (новіші зверху).
        /// </summary>
        public async Task<List<ExpenseLimit>> GetAllLimitsAsync(Guid userId)
        {
            return await _context.ExpenseLimits
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Year)
                .ThenByDescending(e => e.Month)
                .ToListAsync();
        }

        /// <summary>
        /// Видалити ліміт за його Id.
        /// </summary>
        public async Task DeleteLimitAsync(Guid limitId)
        {
            var limit = await _context.ExpenseLimits.FindAsync(limitId);
            if (limit != null)
            {
                _context.ExpenseLimits.Remove(limit);
                await _context.SaveChangesAsync();
            }
        }
    }
}
