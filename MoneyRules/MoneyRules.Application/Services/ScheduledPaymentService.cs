using MoneyRules.Application.Interfaces;
using MoneyRules.Domain.Entities;
using MoneyRules.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MoneyRules.Application.Services
{
    public class ScheduledPaymentService : IScheduledPaymentService
    {
        private readonly AppDbContext _context;

        public ScheduledPaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScheduledPayment> CreateAsync(ScheduledPayment scheduledPayment)
        {
            // Ensure DateTimes are UTC before persisting (Postgres timestamptz expects UTC)
            scheduledPayment.StartDate = NormalizeToUtc(scheduledPayment.StartDate);
            if (scheduledPayment.EndDate.HasValue)
                scheduledPayment.EndDate = NormalizeToUtc(scheduledPayment.EndDate.Value);

            await _context.ScheduledPayments.AddAsync(scheduledPayment);
            await _context.SaveChangesAsync();
            return scheduledPayment;
        }

        public async Task<bool> DeleteAsync(int scheduledPaymentId)
        {
            var sp = await _context.ScheduledPayments.FindAsync(scheduledPaymentId);
            if (sp == null) return false;
            _context.ScheduledPayments.Remove(sp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ScheduledPayment>> GetScheduledPaymentsAsync(int userId)
        {
            return await _context.ScheduledPayments
            .Include(sp => sp.Category)
            .Where(sp => sp.UserId == userId && sp.IsActive)
            .ToListAsync();
        }

        public async Task<ScheduledPayment> UpdateAsync(ScheduledPayment scheduledPayment)
        {
            // Normalize DateTimes to UTC
            scheduledPayment.StartDate = NormalizeToUtc(scheduledPayment.StartDate);
            if (scheduledPayment.EndDate.HasValue)
                scheduledPayment.EndDate = NormalizeToUtc(scheduledPayment.EndDate.Value);

            _context.ScheduledPayments.Update(scheduledPayment);
            await _context.SaveChangesAsync();
            return scheduledPayment;
        }

        private DateTime NormalizeToUtc(DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc)
                return date;

            if (date.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();

            // Local -> ToUniversalTime
            return date.ToUniversalTime();
        }
    }
}
