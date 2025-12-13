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
        public async Task Execute(Message message, ITelegramBotClient botClient)
        {
            try
            {
                _logger.LogInformation("Обработка команды /start для пользователя {UserId}", message.From.Id);
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
