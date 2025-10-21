using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Domain.Enums;

namespace MoneyRules.Application.Services
{
    public class AdviceService : IAdviceService
    {
        // Returns up to 3 tips based on simple heuristics.
        public List<string> GetAdvice(IEnumerable<Transaction> transactions)
        {
            var tips = new List<string>();

            if (transactions == null)
                return tips;

            var txList = transactions.ToList();

            if (!txList.Any())
            {
                tips.Add("Транзакцій не знайдено — почніть відстежувати покупи, щоб отримати персональні поради.");
                return tips;
            }


            var expenseTx = txList.Where(t => t.Type == TransactionType.Expense).ToList();
            if (expenseTx.Any())
            {
                var byCategory = expenseTx.GroupBy(t => t.Category?.Name ?? (t.Description ?? "(Без категорії)"))
                    .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                var top = byCategory.First();
                tips.Add($"Ви витрачаєте найбільше на '{top.Category}' — розгляньте скорочення регулярних покупок або пошук дешевших альтернатив. (Витрачено {top.Total:C})");
            }


            var smallCount = txList.Count(t => Math.Abs(t.Amount) > 0 && Math.Abs(t.Amount) < 10);
            if (smallCount >= 5 && tips.Count < 3)
            {
                tips.Add("Багато дрібних покупок накопичуються — перегляньте щоденні витрати (кава, снеки) і встановіть тижневий ліміт.");
            }


            if (txList.Any())
            {
                var monthly = txList.GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => new { Period = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderBy(p => p.Period.Year).ThenBy(p => p.Period.Month)
                    .ToList();

                if (monthly.Count >= 3)
                {
                    var last = monthly.TakeLast(3).ToList();
                    if (last[2].Total > last[1].Total && last[1].Total > last[0].Total && tips.Count < 3)
                    {
                        tips.Add("Ваші витрати зросли за останні місяці — встановіть місячний бюджет і перегляньте підписки.");
                    }
                }
            }


            var generic = new[] {
                "Складіть список покупок і уникайте імпульсивних покупок.",
                "Скасуйте незатребувані підписки після швидкого аудиту.",
                "Спробуйте переглянути або переговорити тарифи на регулярні платежі (інтернет, мобільний) або перейдіть на дешевший план."
            };

            foreach (var g in generic)
            {
                if (tips.Count >= 3) break;
                if (!tips.Contains(g)) tips.Add(g);
            }

            return tips.Take(3).ToList();
        }
    }
}
