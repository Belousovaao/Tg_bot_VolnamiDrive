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
                return new UserState { UserId = id, StateEnum = BotState.Default};
            });
        }

        public void SetUserState(long userId, BotState state)
        {
            UserState userState = GetUserState(userId);
            userState.StateEnum = state;
            userState.LastActivity = DateTime.UtcNow;
        }
    }
}
