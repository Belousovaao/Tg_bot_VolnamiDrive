using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Models.Domain
{
    public class BotSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public UserState UserState { get; set; } = new();
        public string CurrentCommand { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> SessionData { get; set; } = new();
        public bool IsActive { get; set; } = true;

        public T GetSessionData<T>(string key, T defaultValue = default)
        {
            return SessionData.ContainsKey(key) ? (T)SessionData[key] : defaultValue;
        }

        public void SetSessionData(string key, object value)
        {
            SessionData[key] = value;
            LastActivity = DateTime.UtcNow;
        }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

    }
}
