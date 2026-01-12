using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.StateManagement
{
    public static class SessionManager
    {
        public static readonly Dictionary<long, BotSession> _sessions = new();
        public static readonly object _lock = new object();

        /// <summary>
        /// Возвращает текущую сессию, если её нет - создаёт новую
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static BotSession GetORCreateSession(long userId)
        {
            lock(_lock)
            {
                if (!_sessions.TryGetValue(userId, out var session))
                {
                    session = new BotSession();
                    _sessions[userId] = session;
                }
                return session;
            }
        }

    }
}
