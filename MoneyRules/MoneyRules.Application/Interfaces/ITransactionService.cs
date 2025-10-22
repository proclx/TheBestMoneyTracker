using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(TransactionFilter filter);
        Task UpdateAsync(Transaction transaction);
        Task<List<Category>> GetUserCategoriesAsync(int userId);

    }
}
