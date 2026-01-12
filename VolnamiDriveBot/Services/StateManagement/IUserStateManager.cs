using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;

namespace VolnamiDriveBot.Services.StateManagement
{
    public interface IUserStateManager
    {
        /// <summary>
        /// Получение состояния пользователя по его Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserState GetUserState(long userId);
        
        
        /// <summary>
        /// Устанавливает новое состояние пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="state"></param>
        void SetUserState(long userId, BotState state);
    }
}
