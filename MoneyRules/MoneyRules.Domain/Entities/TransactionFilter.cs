using MoneyRules.Domain.Enums;

namespace MoneyRules.Domain.Entities
{
    public class TransactionFilter
    {
        public int? UserId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TransactionType? Type { get; set; }
    }
}
