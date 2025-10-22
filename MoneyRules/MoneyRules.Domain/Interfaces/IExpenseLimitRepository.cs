using System;
using System.Threading.Tasks;
using MoneyRules.Domain.Entities;

namespace MoneyRules.Domain.Interfaces
{
    public interface IExpenseLimitRepository
    {
        Task SetMonthlyLimitAsync(Guid userId, decimal amount, int year, int month);
        Task<ExpenseLimit?> GetMonthlyLimitAsync(Guid userId, int year, int month);
    }
}
