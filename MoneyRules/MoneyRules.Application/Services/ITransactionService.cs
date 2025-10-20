using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(TransactionFilter filter);
    }
}
