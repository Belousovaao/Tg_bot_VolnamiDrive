using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace VolnamiDriveBot.Services.Calendar
{
    public interface ICalendarManager
    {
        InlineKeyboardMarkup CreateCalendar(int year, int month, Dictionary<DateTime, bool> availableDates, DateTime? selectedStartDate = null, string vehicleId = null);
    }
}
