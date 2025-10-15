using MoneyRules.Domain.Entities;
using System.Collections.Generic;

namespace MoneyRules.Application.Services
{
    public interface IAdviceService
    {
        /// <summary>
        /// Analyze user's transactions and return up to 3 concrete money-saving tips.
        /// </summary>
        /// <param name="transactions">All transactions for the user.</param>
        /// <returns>List of up to 3 advice strings.</returns>
        List<string> GetAdvice(IEnumerable<Transaction> transactions);
    }
}
