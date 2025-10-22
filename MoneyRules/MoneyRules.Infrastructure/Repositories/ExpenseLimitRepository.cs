using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Interfaces; // інтерфейси мають бути тут, не в Application!

namespace MoneyRules.Infrastructure.Repositories
{
    public class ExpenseLimitRepository : IExpenseLimitRepository
    {
        private readonly List<ExpenseLimit> _limits = new();

        public Task SetMonthlyLimitAsync(Guid userId, decimal amount, int year, int month)
        {
            var existing = _limits.FirstOrDefault(x => x.UserId == userId && x.Year == year && x.Month == month);
            if (existing != null)
            {
                existing.Amount = amount;
            }
            else
            {
                _limits.Add(new ExpenseLimit
                {
                    UserId = userId,
                    Amount = amount,
                    Year = year,
                    Month = month
                });
            }

            return Task.CompletedTask;
        }

        public Task<ExpenseLimit?> GetMonthlyLimitAsync(Guid userId, int year, int month)
        {
            var limit = _limits.FirstOrDefault(x => x.UserId == userId && x.Year == year && x.Month == month);
            return Task.FromResult(limit);
        }
    }
}
