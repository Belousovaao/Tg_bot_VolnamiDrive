using Microsoft.Extensions.Logging;
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

        public InlineKeyboardMarkup GenerateVehicalOptionsMenu()
        {

            return new InlineKeyboardMarkup(new[]
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

        public InlineKeyboardMarkup GenerateAfterPriceMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Вернуться в начало", "go_back"),
                    InlineKeyboardButton.WithCallbackData("✅ Перейти к бронированию", "go_booking")
                }
            });
        }

        public InlineKeyboardMarkup GetAdminMainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📊 Статистика", "admin_stats"),
                    InlineKeyboardButton.WithCallbackData("📅 Бронирования", "admin_bookings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🚗 Транспорт", "admin_vehicles"),
                    InlineKeyboardButton.WithCallbackData("👥 Пользователи", "admin_users")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📢 Рассылка", "admin_broadcast"),
                    InlineKeyboardButton.WithCallbackData("⚙️ Настройки", "admin_settings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📈 Аналитика", "admin_analytics"),
                    InlineKeyboardButton.WithCallbackData("🔐 Доступы", "admin_access")
                }
            });
        }

        public InlineKeyboardMarkup GetBackButton(string returnTo = "admin_main")
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("◀️ Назад", returnTo) }
            });
        }

        public InlineKeyboardMarkup AdminAnswer(BookingRequest request)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            // Кнопки для связи если есть контакт
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("📞 Позвонить", $"admin_show_phone_{request.Id}")
                });
            }

            // Другие кнопки
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("💬 Ответить", $"admin_reply_{request.Id}")
            });

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
