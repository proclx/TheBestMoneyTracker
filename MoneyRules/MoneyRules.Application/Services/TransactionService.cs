using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.Date >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.Date <= filter.ToDate.Value);

            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);

            return await query.OrderByDescending(t => t.Date).ToListAsync();
        }
    }
}
