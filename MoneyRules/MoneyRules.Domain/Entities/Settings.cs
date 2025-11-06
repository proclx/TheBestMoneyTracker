namespace MoneyRules.Domain.Entities
{
    public class Settings
    {
        public int UserId { get; set; } // PK and FK
        public string Currency { get; set; }
        public bool NotificationEnabled { get; set; }

        // --- ДОДАНО ДЛЯ ТЕМИ ---
        public string Theme { get; set; } = "Light"; // За замовчуванням - світла

        // Navigation
        public User User { get; set; }
    }
}