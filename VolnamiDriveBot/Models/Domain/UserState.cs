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
        //public int MessageCount { get; set; }

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
                LastActivity = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Получает данные состояния пользователя в словарь по ключу и значению
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetData<T>(string key, T defaultValue = default)
        {
            return Data.ContainsKey(key) ? (T)Data[key] : defaultValue;
        }


        /// <summary>
        /// Устанавливает данные состояния пользователя в словарь по ключу и значению
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetData(string key, object value)
        {
            Data[key] = value;
        }

        /// <summary>
        /// Удаляет данные из словаря по ключу
        /// </summary>
        /// <param name="key"></param>
        public void RemoveData(string key)
        {
            Data.Remove(key);
        }
    }
}
