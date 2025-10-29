using MoneyRules.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoneyRules.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<ExchangeRate>> GetCurrentExchangeRatesAsync();
    }
}