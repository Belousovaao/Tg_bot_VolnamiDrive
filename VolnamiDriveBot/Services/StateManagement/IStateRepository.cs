using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.StateManagement
{
    public interface IStateRepository
    {
        Task<UserState> GetUserStateAsync(long userId);
        Task SaveUserStateAsync(UserState userState);
        Task DeleteUserStateAsync(long userId);
        Task<IEnumerable<UserState>> GetAllUserStatesAsync();
        Task CleanupExpiredStatesAsync(TimeSpan expirationTime);
    }
}
