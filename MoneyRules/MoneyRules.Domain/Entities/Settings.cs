namespace MoneyRules.Domain.Entities
{
    public class Settings
    {
        public int UserId { get; set; } // PK and FK
        public string Currency { get; set; }
        public bool NotificationEnabled { get; set; }

        // Navigation
        public User User { get; set; }
    }
}
