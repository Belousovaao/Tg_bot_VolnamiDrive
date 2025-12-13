using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolnamiDriveBot.Models.Enums;

namespace VolnamiDriveBot.Models.Domain
{
    public class UserState
    {
        public long UserId { get; set; }
        public string CurrentState { get; set; } = "default";
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public int MessageCount { get; set; }

        public BotState StateEnum
        {
            get
            {
                if (Enum.TryParse<BotState>(CurrentState, out var state))
                    return state;
                return BotState.Default;
            }
            set
            {
                CurrentState = value.ToString();
                LastActivity = DateTime.UtcNow; // Обновляем активность
            }
        }


        public T GetData<T>(string key, T defaultValue = default)
        {
            return Data.ContainsKey(key) ? (T)Data[key] : defaultValue;
        }

        public void SetData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
