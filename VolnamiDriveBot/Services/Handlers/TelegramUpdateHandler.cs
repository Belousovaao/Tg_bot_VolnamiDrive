using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;

namespace VolnamiDriveBot.Services.Handlers
{
    public class TelegramUpdateHandler : IUpdateHandler
    {
        private readonly ILogger<TelegramUpdateHandler> _logger;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        private readonly MessageHandler _messageHandler;


        public TelegramUpdateHandler(ILogger<TelegramUpdateHandler> logger, MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler)
        {
            _logger = logger;
            _messageHandler = messageHandler;
            _callbackQueryHandler = callbackQueryHandler;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            _logger.LogDebug("📨 Получено обновление: {UpdateType}", update.Type);

            try
            {
                switch(update.Type)
                {
                    case UpdateType.Message:
                        await _messageHandler.HandleMessageAsync(botClient, update.Message!, ct);
                        break;
                    case UpdateType.CallbackQuery:
                        await _callbackQueryHandler.HandleCallbackQueryAsync(botClient, update.CallbackQuery!, ct);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка обработки обновления {UpdateType}", update.Type);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "🔥 Критическая ошибка в обработчике");
            return Task.CompletedTask;
        }
    }
}