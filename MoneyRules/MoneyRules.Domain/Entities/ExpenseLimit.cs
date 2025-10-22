using System;

namespace MoneyRules.Domain.Entities
{
    public class ExpenseLimit
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // ✅ Порожній конструктор — потрібен для EF Core і для new ExpenseLimit { ... }
        public ExpenseLimit() { }

        // ✅ Твій зручний конструктор з параметрами
        public ExpenseLimit(Guid userId, decimal amount, int year, int month)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Amount = amount;
            Year = year;
            Month = month;
        }
    }
}
