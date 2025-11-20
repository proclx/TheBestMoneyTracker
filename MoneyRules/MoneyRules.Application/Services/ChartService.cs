using MoneyRules.Domain.Entities;
using MoneyRules.Application.Interfaces;
using MoneyRules.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoneyRules.Application.Services
{
    public class ChartService : IChartService
    {
        private readonly AppDbContext _dbContext;

        public ChartService(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public List<int> GetTransactionYears(int userId)
        {
            try
            {
                return _dbContext.Transactions
                    .Where(t => t.UserId == userId)
                    .Select(t => t.Date.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<int>();
            }
        }

        public Dictionary<int, (decimal Income, decimal Expense)> GetMonthlyStatistics(int userId, int year)
        {
            var result = new Dictionary<int, (decimal Income, decimal Expense)>();
            
            try
            {
                var transactions = _dbContext.Transactions
                    .Where(t => t.UserId == userId && t.Date.Year == year)
                    .ToList();

                for (int month = 1; month <= 12; month++)
                {
                    var monthlyTransactions = transactions.Where(t => t.Date.Month == month);
                    
                    var income = monthlyTransactions
                        .Where(t => t.Type.ToString().ToLower().Contains("income"))
                        .Sum(t => t.Amount);
                    
                    var expense = monthlyTransactions
                        .Where(t => t.Type.ToString().ToLower().Contains("expense"))
                        .Sum(t => t.Amount);

                    result[month] = (income, expense);
                }
            }
            catch (Exception)
            {
                for (int month = 1; month <= 12; month++)
                {
                    result[month] = (0, 0);
                }
            }

            return result;
        }

        public Dictionary<int, (decimal Income, decimal Expense)> GetDailyStatistics(int userId, int year, int month)
        {
            var result = new Dictionary<int, (decimal Income, decimal Expense)>();

            try
            {
                // Guard month
                if (month < 1 || month > 12)
                {
                    return result;
                }

                var transactions = _dbContext.Transactions
                    .Where(t => t.UserId == userId && t.Date.Year == year && t.Date.Month == month)
                    .ToList();

                var daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var dailyTransactions = transactions.Where(t => t.Date.Day == day);

                    var income = dailyTransactions
                        .Where(t => t.Type.ToString().ToLower().Contains("income"))
                        .Sum(t => t.Amount);

                    var expense = dailyTransactions
                        .Where(t => t.Type.ToString().ToLower().Contains("expense"))
                        .Sum(t => t.Amount);

                    result[day] = (income, expense);
                }
            }
            catch (Exception)
            {
                // On error return zeroed days for the requested month if the month was valid
                if (month >= 1 && month <= 12)
                {
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        result[day] = (0, 0);
                    }
                }
            }

            return result;
        }

        public Dictionary<string, decimal> GetCategoryTotals(int userId, int? year = null, int? month = null)
        {
            try
            {
                var query = _dbContext.Transactions.AsQueryable()
                    .Where(t => t.UserId == userId);

                if (year.HasValue)
                    query = query.Where(t => t.Date.Year == year.Value);

                if (month.HasValue)
                    query = query.Where(t => t.Date.Month == month.Value);

                var grouped = query
                    .Where(t => t.Type.ToString().ToLower().Contains("expense"))
                    .ToList()
                    .GroupBy(t => t.Category?.Name ?? (t.Description ?? "(Без категорії)"))
                    .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Total)
                    .ToDictionary(x => x.Category, x => x.Total);

                return grouped;
            }
            catch (Exception)
            {
                return new Dictionary<string, decimal>();
            }
        }
    }
}
