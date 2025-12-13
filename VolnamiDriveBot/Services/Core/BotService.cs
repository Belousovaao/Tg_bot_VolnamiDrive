using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Logging;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Services.Calendar;

namespace VolnamiDriveBot.Services.Core
{
    public class BotService : IBotService

    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUpdateHandler _updateHandler;
        private readonly ILogger<BotService> _logger;

        public BotService(ITelegramBotClient botClient, IUpdateHandler updateHandler, ILogger<BotService> logger)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
            _logger = logger;
        }

        /// <summary>
        /// Запускает бот и процесс получениия обновлений от тг в фоновом режиме асинхренно.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartBot(CancellationToken cancellationToken)
        {
            _botClient.StartReceiving(
            updateHandler: _updateHandler,
            receiverOptions: new ReceiverOptions(),
            cancellationToken: cancellationToken);

            _logger.LogInformation("Сервис бота запущен");
        }
    }
}