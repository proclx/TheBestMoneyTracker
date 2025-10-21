using MoneyRules.Domain.Enums;

namespace MoneyRules.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public byte[] ProfilePhoto { get; set; }

        // One-to-One
        public Settings Settings { get; set; }

        // One-to-Many
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
