using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;
using static VolnamiDriveBot.Services.Core.BotService;

namespace VolnamiDriveBot.Services.StateManagement
{
    public class UserStateManager : IUserStateManager
    {
        private readonly ConcurrentDictionary<long, UserState> _userStates = new();
        private readonly ILogger<UserStateManager> _logger;

        public UserStateManager(ILogger<UserStateManager> logger)
        {
            _logger = logger;
        }

        public UserState GetUserState(long userId)
        {
            return _userStates.GetOrAdd(userId, id =>
            {
                _logger.LogDebug("🆕 Создано состояние для пользователя {UserId}", id);
                return new UserState { UserId = id, StateEnum = BotState.Default};
            });
        }

        public void SetUserState(long userId, BotState state)
        {
            UserState userState = GetUserState(userId);
            BotState oldState = userState.StateEnum;
            userState.StateEnum = state;
            userState.LastActivity = DateTime.UtcNow;

            _logger.LogDebug("🔄 Пользователь {UserId}: {OldState} -> {NewState}",userId, oldState, state);
        }

        void IUserStateManager.SetUserData(long userId, string key, object value)
        {
            UserState userState = GetUserState(userId);
            userState.SetData(key, value);
            _logger.LogDebug("💾 Сохранены данные для {UserId}: {Key} = {Value}", userId, key, value);
        }

        T IUserStateManager.GetUserData<T>(long userId, string key)
        {
            UserState userState = GetUserState(userId);
            var value = userState.GetData<T>(key);
            _logger.LogDebug("📖 Получены данные для {UserId}: {Key} = {Value}", userId, key, value);
            return value;
        }
    }
}
