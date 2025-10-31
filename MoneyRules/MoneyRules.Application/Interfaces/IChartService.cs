using MoneyRules.Domain.Entities;
using System.Collections.Generic;

namespace MoneyRules.Application.Interfaces
{
    public interface IChartService
    {
        List<int> GetTransactionYears(int userId);
        Dictionary<int, (decimal Income, decimal Expense)> GetMonthlyStatistics(int userId, int year);
        Dictionary<int, (decimal Income, decimal Expense)> GetDailyStatistics(int userId, int year, int month);
    }
}