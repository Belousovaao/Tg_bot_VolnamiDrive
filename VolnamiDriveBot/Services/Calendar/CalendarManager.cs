using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace VolnamiDriveBot.Services.Calendar
{
    public class CalendarManager : ICalendarManager
    {
        private readonly string[] _monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
        private readonly string[] _weekDays = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

        public InlineKeyboardMarkup CreateCalendar(int year, int month, Dictionary<DateTime, bool> availableDates, DateTime? selectedStartDate = null, string vehicleId = null)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            // Заголовок с навигацией
            string headerText = selectedStartDate.HasValue
           ? $"🗓️ Выберите дату ОКОНЧАНИЯ"
           : "🗓️ Выберите дату НАЧАЛА";

            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData("◀️", $"cal_prev_{year}_{month}_{(selectedStartDate?.ToString("yyyy-MM-dd") ?? "null")}"),
            InlineKeyboardButton.WithCallbackData($"{_monthNames[month-1]} {year} 📅", "ignore"),
            InlineKeyboardButton.WithCallbackData("▶️", $"cal_next_{year}_{month}_{(selectedStartDate?.ToString("yyyy-MM-dd") ?? "null")}")
            });

            // Дни недели с иконками
            var weekDayButtons = _weekDays.Select(day =>
                InlineKeyboardButton.WithCallbackData(day, "ignore")).ToArray();
            buttons.Add(weekDayButtons);

            // Дни месяца
            var firstDay = new DateTime(year, month, 1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var startDay = ((int)firstDay.DayOfWeek + 6) % 7; // Понедельник = 0

            var currentRow = new List<InlineKeyboardButton>();

            // Пустые кнопки до первого дня месяца
            for (int i = 0; i < startDay; i++)
            {
                currentRow.Add(InlineKeyboardButton.WithCallbackData("⋅", "ignore"));
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDate = new DateTime(year, month, day);
                var isToday = currentDate.Date == DateTime.Today;
                var isAvailable = availableDates.ContainsKey(currentDate.Date) && availableDates[currentDate.Date];
                var isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                var isValidEndDate = selectedStartDate.HasValue &&
                               currentDate >= selectedStartDate.Value &&
                               IsDateRangeAvailable(selectedStartDate.Value, currentDate, availableDates);
                var (emoji, callback) = GetDayButtonInfo(currentDate, isAvailable, isToday, isWeekend, selectedStartDate, isValidEndDate);

                currentRow.Add(InlineKeyboardButton.WithCallbackData($"{emoji} {day}", callback));

                if (currentRow.Count == 7 || day == daysInMonth)
                {
                    buttons.Add(currentRow.ToArray());
                    currentRow.Clear();
                }
            }

            // Информационная строка
            if (selectedStartDate.HasValue)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"📅 Начало: {selectedStartDate:dd.MMM}", "ignore")
                });
            }

            // Кнопки навигации
            if (selectedStartDate.HasValue)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔄 Сбросить", "cal_reset")
                });
            }
            else
            {
                // 📅 РЕЖИМ ВЫБОРА ДАТЫ НАЧАЛА
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("🎯 Сегодня", $"cal_today_null"),
                });
            }


                return new InlineKeyboardMarkup(buttons);
        }

        private (string emoji, string callback) GetDayButtonInfo(DateTime date, bool isAvailable, bool isToday, bool isWeekend, DateTime? selectedStartDate, bool isValidEndDate)
        {
            if (!isAvailable)
            {
                return ("❌", "cal_unavailable");
            }    

            if (selectedStartDate.HasValue)
            {
                if (!isValidEndDate)
                    return ("❌", "cal_unavailable_range");

                return ("✅", $"cal_end_{date:yyyy-MM-dd}");
            }
            else
            {
                // Режим выбора даты начала
                if (isToday)
                    return ("🎯", $"cal_start_{date:yyyy-MM-dd}");

                return ("✅", $"cal_start_{date:yyyy-MM-dd}");
            }
        }

        private bool IsDateRangeAvailable(DateTime startDate, DateTime endDate, Dictionary<DateTime, bool> availableDates)
        {
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!availableDates.ContainsKey(date) || !availableDates[date])
                    return false;
            }
            return true;
        }
    }
}
