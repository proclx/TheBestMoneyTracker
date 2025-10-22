using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface IExpenseLimitRepository
    {
        Task SetLimitAsync(ExpenseLimit limit);
        Task<ExpenseLimit?> GetLimitAsync(Guid userId, int year, int month);
    }
}
