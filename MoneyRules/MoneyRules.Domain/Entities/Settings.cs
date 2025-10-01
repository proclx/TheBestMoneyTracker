namespace MoneyRules.Domain.Entities
{
    public class Settings
    {
        public int SettingsId { get; set; }
        public string Currency { get; set; }
        public bool NotificationEnabled { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
