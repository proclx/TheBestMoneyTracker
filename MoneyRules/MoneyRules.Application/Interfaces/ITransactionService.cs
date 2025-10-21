using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(TransactionFilter filter);
    }
}
