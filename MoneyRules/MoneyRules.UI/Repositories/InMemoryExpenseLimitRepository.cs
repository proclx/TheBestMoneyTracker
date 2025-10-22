using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;

namespace MoneyRules.UI.Repositories
{
    public class InMemoryExpenseLimitRepository : IExpenseLimitRepository
    {
        private readonly List<ExpenseLimit> _limits = new();

        public Task SetLimitAsync(ExpenseLimit limit)
        {
            var existing = _limits.FirstOrDefault(l => l.UserId == limit.UserId && l.Year == limit.Year && l.Month == limit.Month);
            if (existing != null)
                _limits.Remove(existing);

            _limits.Add(limit);
            return Task.CompletedTask;
        }

        public Task<ExpenseLimit?> GetLimitAsync(Guid userId, int year, int month)
        {
            var limit = _limits.FirstOrDefault(l => l.UserId == userId && l.Year == year && l.Month == month);
            return Task.FromResult(limit);
        }
    }
}
