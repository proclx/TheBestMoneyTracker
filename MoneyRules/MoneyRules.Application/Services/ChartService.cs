using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace MoneyRules.Application.Services
{
    public class ChartService
    {
        // Returns 12 months with income and expense totals for a user and year
        public List<(int Month, decimal Income, decimal Expense)> GetMonthlyTotals(AppDbContext db, int userId, int year)
        {
            var months = Enumerable.Range(1, 12).Select(m =>
            {
                var income = db.Transactions.Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == m && t.Type.ToString().ToLower().Contains("income")).Sum(t => (decimal?)t.Amount) ?? 0m;
                var expense = db.Transactions.Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == m && t.Type.ToString().ToLower().Contains("expense")).Sum(t => (decimal?)t.Amount) ?? 0m;
                return (Month: m, Income: income, Expense: expense);
            }).ToList();

            return months;
        }
    }
}
