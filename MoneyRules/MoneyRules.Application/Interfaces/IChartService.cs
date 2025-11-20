using MoneyRules.Domain.Entities;
using System.Collections.Generic;

namespace MoneyRules.Application.Interfaces
{
    public interface IChartService
    {
        List<int> GetTransactionYears(int userId);
        Dictionary<int, (decimal Income, decimal Expense)> GetMonthlyStatistics(int userId, int year);
        Dictionary<int, (decimal Income, decimal Expense)> GetDailyStatistics(int userId, int year, int month);
        // Повертає сумарні витрати (за замовчуванням) або доходи за категоріями
        // Якщо вказано year і month - фільтрує по місяцю; якщо вказано лише year - по року; якщо нічого не вказано - по всьому періоду
        Dictionary<string, decimal> GetCategoryTotals(int userId, int? year = null, int? month = null);
    }
}