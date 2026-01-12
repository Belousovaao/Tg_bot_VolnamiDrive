using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VolnamiDriveBot.Services.Keyboard;

namespace VolnamiDriveBot.Commands.Concrete
{
    public class StartCommand : ICommand
    {

        private readonly ILogger<StartCommand> _logger;
        private readonly IKeyboardService _keyboardService;

        public StartCommand(ILogger<StartCommand> logger, IKeyboardService keyboardService)
        {
            _logger = logger;
            _keyboardService = keyboardService;
        }

        /// <summary>
        /// Отправляет стартовое меню выбора авто/мото
        /// </summary>
        /// <param name="message"> - входящее сообщение пользователя вида "/statrt"</param>
        /// <param name="botClient"> - IBotClient индентификатор клиента</param>
        /// <returns></returns>
        public async Task Execute(Message message, ITelegramBotClient botClient)
        {
            try
            {
                await botClient.SendPhoto(
                message.Chat.Id,
                InputFile.FromUri("https://freeimage.host/i/fB8dAas"),
                "<b>Привет, меня зовут Волник!</b>\n\nХочешь посмотреть мой автопарк или мотопарк?",
                ParseMode.Html,
                replyMarkup: _keyboardService.StartMenuKeyboard());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка выполнения команды /start");
            }
        }
    }
}
