using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;

namespace VolnamiDriveBot.Services.StateManagement
{
    public interface IUserStateManager
    {
        UserState GetUserState(long userId);
        void SetUserState(long userId, BotState state);
        void SetUserData(long userId, string key, object value);
        T GetUserData<T>(long userId, string key);
    }
}
