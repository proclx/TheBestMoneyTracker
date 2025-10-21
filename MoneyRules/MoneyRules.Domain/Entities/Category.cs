using MoneyRules.Domain.Enums;

namespace MoneyRules.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Name { get; set; }
        public CategoryType Type { get; set; }

        // Optional: collection of Transactions in this category
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
