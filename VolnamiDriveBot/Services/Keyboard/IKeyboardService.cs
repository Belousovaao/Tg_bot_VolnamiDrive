using Telegram.Bot.Types.ReplyMarkups;
using VolnamiDriveBot.Models.Domain;
namespace VolnamiDriveBot.Services.Keyboard
{
    public interface IKeyboardService
    {
        /// <summary>
        /// Клавиатура выбора авто или мото на стартовом меню
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup StartMenuKeyboard();

        /// <summary>
        /// Клавиатура со списком всех тс из выбранной категории (авто/мото) + кнопка оставить пожалания
        /// </summary>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        InlineKeyboardMarkup GenerateVehiclesMenu(string vehicleType);

        /// <summary>
        /// клавиатура для каждого тс с кнопками посмотреть и рассчитать стоимость
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup GenerateVehicalOptionsMenu();

        /// <summary>
        /// клавиатура после подробной информации об авто
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup GenerateMoreMenu();

        /// <summary>
        /// клавиатура, выпадающая после получения прайса
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup GenerateAfterPriceMenu();

        /// <summary>
        /// админское меню, выпадает посл команды /admin
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup GetAdminMainMenu();


        InlineKeyboardMarkup GetBackButton(string returnTo = "admin_main");
        
        InlineKeyboardMarkup AdminAnswer(BookingRequest request);
    }
}
