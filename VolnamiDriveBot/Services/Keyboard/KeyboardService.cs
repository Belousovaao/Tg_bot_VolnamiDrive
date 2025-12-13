using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Services.VehicleService;

namespace VolnamiDriveBot.Services.Keyboard
{
    public class KeyboardService : IKeyboardService
    {
        private readonly IVehicleService _vehicleService;

        public KeyboardService(IVehicleService vehicleService, ILogger<KeyboardService> logger)
        {
            _vehicleService = vehicleService;
        }
        public InlineKeyboardMarkup StartMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
        {
            new[]
                {
                    InlineKeyboardButton.WithCallbackData("🚗Автопарк", "auto"),
                    InlineKeyboardButton.WithCallbackData("🏍Мотопарк", "moto")
                }

        });
        }

        public InlineKeyboardMarkup GenerateVehiclesMenu(string vehicleType)
        {
            Dictionary<string, Vehicle> vehicles = _vehicleService.GetVehiclesByType(vehicleType);

            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

            foreach (Vehicle vehicle in vehicles.Values)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(vehicle.Description, vehicle.Id)
                });
            }

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Хочу оставить пожелание по модели.", "wishes") });

            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура для каждого тс с кнопками посмотреть и рассчитать стоимость
        public InlineKeyboardMarkup GenerateVehicalOptionsMenu()
        {
            
            return new InlineKeyboardMarkup (new[] 
            {
                new[] 
                {
                    InlineKeyboardButton.WithCallbackData("👀Подробнее", "more"),
                    InlineKeyboardButton.WithCallbackData("💰Рассчитать стоимость", "price")
                }
            });
        }

        public InlineKeyboardMarkup GenerateMoreMenu()
        {
            return new InlineKeyboardMarkup(new[]
            { 
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Вернуться в начало", "go_back"),
                    InlineKeyboardButton.WithCallbackData("💰Рассчитать стоимость", "price")
                }
            });
        }
    }
}
