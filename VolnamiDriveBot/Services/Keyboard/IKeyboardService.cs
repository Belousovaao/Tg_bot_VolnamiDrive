using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace VolnamiDriveBot.Services.Keyboard
{
    public interface IKeyboardService
    {
        InlineKeyboardMarkup StartMenuKeyboard();
        InlineKeyboardMarkup GenerateVehiclesMenu(string vehicleType);
        InlineKeyboardMarkup GenerateVehicalOptionsMenu();
        InlineKeyboardMarkup GenerateMoreMenu();
    }
}
