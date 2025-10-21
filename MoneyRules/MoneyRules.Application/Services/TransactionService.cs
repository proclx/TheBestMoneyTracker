using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MoneyRules.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(TransactionFilter filter)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.User)
                .AsQueryable();

            if (filter.UserId.HasValue)
                query = query.Where(t => t.UserId == filter.UserId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

            // 🕒 Безпечне перетворення у UTC
            if (filter.FromDate.HasValue)
            {
                var fromUtc = NormalizeToUtc(filter.FromDate.Value);
                query = query.Where(t => t.Date >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = NormalizeToUtc(filter.ToDate.Value);
                query = query.Where(t => t.Date <= toUtc);
            }

            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync();
        }

        private DateTime NormalizeToUtc(DateTime date)
        {
            // Якщо дата без "Kind" — вважаємо її локальною і конвертуємо у UTC
            if (date.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();

            // Якщо локальна — просто переводимо у UTC
            if (date.Kind == DateTimeKind.Local)
                return date.ToUniversalTime();

            // Якщо вже UTC — нічого не робимо
            return date;
        }
    }
}
