using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
 public interface IScheduledPaymentService
 {
 Task<IEnumerable<ScheduledPayment>> GetScheduledPaymentsAsync(int userId);
 Task<ScheduledPayment> CreateAsync(ScheduledPayment scheduledPayment);
 Task<bool> DeleteAsync(int scheduledPaymentId);
 Task<ScheduledPayment> UpdateAsync(ScheduledPayment scheduledPayment);
 }
}
