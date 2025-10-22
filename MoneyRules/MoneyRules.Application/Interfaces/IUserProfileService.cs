using MoneyRules.Domain.Entities;

namespace MoneyRules.Application.Interfaces
{
    public interface IUserProfileService
    {
        User GetUserById(int id);
        void UpdateUser(User user);
        void ChangeProfilePhoto(User user, byte[] photoData);
    }
}
