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
        public int SettingsId { get; set; }
        public Settings Settings { get; set; }
    }
}
