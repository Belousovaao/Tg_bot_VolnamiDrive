using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using VolnamiDriveBot.Commands;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;
using VolnamiDriveBot.Services.Commands;
using VolnamiDriveBot.Services.Keyboard;
using VolnamiDriveBot.Services.StateManagement;

namespace VolnamiDriveBot.Services.Handlers
{
    public class MessageHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger<MessageHandler> _logger;
        private readonly IKeyboardService _startmenu;

        public MessageHandler(IUserStateManager stateManager, ICommandFactory commandFactory, ILogger<MessageHandler> logger, IKeyboardService startMenu)
        {
            _stateManager = stateManager;
            _commandFactory = commandFactory;
            _logger = logger;
            _startmenu = startMenu;
        }
        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
        {

            if (message.Text == null) return;

            long userId = message.From!.Id;
            UserState userState = _stateManager.GetUserState(userId);
            _logger.LogDebug("👤 Состояние пользователя {UserId}: {CurrentState}",userId, userState.CurrentState);

            if (userState.StateEnum == BotState.AwaitingWishes)
            {
                _logger.LogInformation("Пользователь {UserId} прислал пожелание: {Wish}", userId, message.Text);
                try
                {
                    //Сохраняем пожелание для аналитики
                    var existingWishes = _stateManager.GetUserData<List<string>>(userId, "wishes") ?? new List<string>();
                    existingWishes.Add(message.Text);
                    _stateManager.SetUserData(userId, "wishes", existingWishes);


                    await botClient.SendMessage(
                        message.Chat.Id,
                        "✅ Спасибо за твои пожелания!\n\nЯ обязательно учту их при пополнении своего парка. Возвращайся к выбору транспорта!",
                        replyMarkup: _startmenu.StartMenuKeyboard(),
                        cancellationToken: ct);

                    //Возвращаем в обычное состояние
                    _stateManager.SetUserState(userId, BotState.Default);
                    return;
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Ошибка сохранения пожелания пользователя {UserId}", userId);
                    return;
                }
            } 

            ICommand command = _commandFactory.CreateCommand(message.Text, userState.CurrentState);

            if (command != null)
            {
                try
                {
                    await command.Execute(message, botClient);
                }

                catch(Exception ex)
                {
                    _logger.LogError(ex, "❌ Ошибка выполнения команды {CommandType}", command.GetType().Name);

                    // Отправляем пользователю сообщение об ошибке
                    await botClient.SendMessage(
                        message.Chat.Id,
                        "⚠️ Произошла ошибка при выполнении команды. Попробуйте позже.",
                        cancellationToken: ct);
                }
            }
            else
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "❌ Команда не распознана. Используйте /help для списка команд.",
                    cancellationToken: ct);
            }
        }
    }
}
