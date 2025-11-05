using MoneyRules.Domain.Enums;

namespace MoneyRules.Domain.Entities
{
 public class ScheduledPayment
 {
 public int ScheduledPaymentId { get; set; }

 public int UserId { get; set; }
 public User User { get; set; }

 // Optional category
 public int? CategoryId { get; set; }
 public Category? Category { get; set; }

 public decimal Amount { get; set; }
 public string Description { get; set; }

 public DateTime StartDate { get; set; }
 public DateTime? EndDate { get; set; }

 public ScheduledPaymentFrequency Frequency { get; set; }
 // Interval allows e.g. every2 months, every3 weeks, etc.
 public int Interval { get; set; } =1;

 public bool IsActive { get; set; } = true;
 }
}
