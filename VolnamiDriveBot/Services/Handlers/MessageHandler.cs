using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VolnamiDriveBot.Commands;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;
using VolnamiDriveBot.Services.Commands;
using VolnamiDriveBot.Services.Keyboard;
using VolnamiDriveBot.Services.StateManagement;
using VolnamiDriveBot.Services.Admin;
using VolnamiDriveBot.Services.BookingService;

namespace VolnamiDriveBot.Services.Handlers
{
    public class MessageHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger<MessageHandler> _logger;
        private readonly IKeyboardService _keyboardService;
        private readonly IAdminService _adminService;
        private readonly IBookingService _bookingService;
        private readonly AdminCallbackHandler _adminCallback;

        public MessageHandler(IUserStateManager stateManager, ICommandFactory commandFactory,
            ILogger<MessageHandler> logger, IKeyboardService keyboardService,
            IAdminService adminService, IBookingService bookingService, AdminCallbackHandler adminCallback)
        {
            _stateManager = stateManager;
            _commandFactory = commandFactory;
            _logger = logger;
            _keyboardService = keyboardService;
            _adminService = adminService;
            _bookingService = bookingService;
            _adminCallback = adminCallback;
        }
        /// <summary>
        /// Обработчик текстовых сообщений, либо полученных фото/файлов в боте
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
        {

            if (message.Text == null && message.Photo == null && message.Document == null) return;

            long userId = message.From!.Id;
            UserState userState = _stateManager.GetUserState(userId);

            // если бот в состоянии ожидания контактных данных, сообщение конвертируется в контактные данные,
            // если успешно, то отправляется ответное сообщение пользователю
            if (userState.StateEnum == BotState.AwaitingContactInfo && !string.IsNullOrEmpty(message.Text))
            {
                await HandleContactInfoInput(botClient, message, ct);
                return;
            }

            // если бот в состоянии ожидания пожеланий, то полученное сообщение сохраняется и отправляется админу, а пользователь возвращается к стартовому меню
            if (userState.StateEnum == BotState.AwaitingWishes)
            {
                _logger.LogInformation("Пользователь {UserId} прислал пожелание: {Wish}", userId, message.Text);
      
                try
                {
                    //Сохраняем пожелание для аналитики
                    UserState currentUserState = _stateManager.GetUserState(userId);
                    var existingWishes = currentUserState.GetData<List<string>>("wishes") ?? new List<string>();
                    existingWishes.Add(message.Text);
                    currentUserState.SetData("wishes", existingWishes);

                    // Уведомляем администраторов
                    await _adminService.NotifyNewMessage(userId, message.From.Username, message.Text);

                    await botClient.SendMessage(
                        message.Chat.Id,
                        "✅ Спасибо за твои пожелания!\n\nЯ обязательно учту их при пополнении своего парка. Возвращайся к выбору транспорта!",
                        replyMarkup: _keyboardService.StartMenuKeyboard(),
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

            //если бот получил фото или документ, вызывается обработчик документов
            if (message.Photo != null || message.Document != null)
            {
                _logger.LogInformation("📸 Получен документ от пользователя {UserId}", userId);
                await HandleDocumentAsync(botClient, message, userState, ct);
                return; 
            }

            // если получено сообщение /admin и польователь является админом, то он получает админскую панель
            if (message.Text.StartsWith("/admin"))
            {
                if (!_adminService.IsAdmin(userId)) return;

                await botClient.SendMessage(message.Chat.Id,
                    "👑 *Панель администратора*\n" +
                    "Выберите раздел для управления:",
                    ParseMode.Markdown,
                    replyMarkup: _keyboardService.GetAdminMainMenu());

                return;
            }

            // в остальных случаях текстовое сообщение пытается быть преобразованным в команду через фабрику
            ICommand command = _commandFactory.CreateCommand(message.Text, userState.CurrentState);
            
            // если такая команда существует, выполняем её
            if (command != null)
            {
                try
                {
                    await command.Execute(message, botClient);
                }

                catch (Exception ex)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        "Произошла ошибка при выполнении команды. Попробуйте позже.",
                        cancellationToken: ct);
                }
            }
        }

        /// <summary>
        /// Обрабатывает фото/файл, который пользователь прислал в бот
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <param name="userState"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task HandleDocumentAsync(ITelegramBotClient botClient, Message message, UserState userState, CancellationToken ct)
        {
            // пот получает фото и созраняет (в макс. качестве) его идентификатор
            long userId = message.From.Id;
            BotSession session = SessionManager.GetORCreateSession(userId);
            string? requestId = session?.GetSessionData<string>("CurrentBookingRequestId");

            if (string.IsNullOrEmpty(requestId))
            {
                _logger.LogWarning("Пользователь {UserId} отправил документ без активной заявки", userId);
                return;
            }

            string? fileId = null;
            if (message.Photo != null && message.Photo.Length > 0)
            {
                fileId = message.Photo.Last().FileId;
            }
            else if (message.Document != null)
            {
                fileId = message.Document.FileId;
            }

            if (string.IsNullOrEmpty(fileId))
            {
                _logger.LogWarning("Не удалось получить fileId от пользователя {UserId}", userId);
                return;
            }

            // далее в зависимости от текущего состояния, добавляет этот идентификатор в соответствующее поле брони
            try
            {
                switch (userState.StateEnum)
                {
                    case BotState.AwaitingPassportPhoto:
                        
                        await _bookingService.AddPassportMainPhoto(requestId, fileId);

                        await botClient.SendMessage(
                            message.Chat.Id,
                            "✅ <b>Разворот с фото принят!</b>\n\n" +
                            "Теперь отправьте, пожалуйста, разворот с пропиской (страницы 5-6):",
                            ParseMode.Html,
                            cancellationToken: ct);

                        _stateManager.SetUserState(userId, BotState.AwaitingPassportRegistration);
                        _logger.LogInformation("Пользователь {UserId} отправил фото паспорта (основное) для заявки {RequestId}",
                            userId, requestId);
                        break;

                    case BotState.AwaitingPassportRegistration:
                        await _bookingService.AddPassportRegistrationPhoto(requestId, fileId);

                        await botClient.SendMessage(
                            message.Chat.Id,
                            "✅ <b>Разворот с пропиской принят!</b>\n\n" +
                            "Теперь отправьте, пожалуйста, фото <b>лицевой стороны</b> водительского удостоверения:",
                            ParseMode.Html,
                            cancellationToken: ct);

                        _stateManager.SetUserState(userId, BotState.AwaitingDrivingLicenseFrontPhoto);
                        _logger.LogInformation("Пользователь {UserId} отправил фото паспорта (прописка) для заявки {RequestId}",
                            userId, requestId);
                        break;

                    case BotState.AwaitingDrivingLicenseFrontPhoto:
                        await _bookingService.AddDrivingLicenseFrontPhoto(requestId, fileId);

                        await botClient.SendMessage(
                            message.Chat.Id,
                            "✅ <b>Лицевая сторона ВУ принята!</b>\n\n" +
                            "Теперь отправьте, пожалуйста, фото <b>оборотной стороны</b> водительского удостоверения:",
                            ParseMode.Html,
                            cancellationToken: ct);

                        _stateManager.SetUserState(userId, BotState.AwaitingDrivingLicenseBackPhoto);
                        _logger.LogInformation("Пользователь {UserId} отправил фото ВУ (лицевая) для заявки {RequestId}",
                            userId, requestId);
                        break;

                    case BotState.AwaitingDrivingLicenseBackPhoto:
                        await _bookingService.AddDrivingLicenseBackPhoto(requestId, fileId);

                        await botClient.SendMessage(
                            message.Chat.Id,
                            "🎉 <b>Все документы получены!</b>\n\n" +
                            "📞 <b>Для связи укажите, пожалуйста:</b>\n" +
                            "• Ваш username в Telegram (например, @username)\n" +
                            "• Или номер телефона\n\n" +
                            "Это нужно для быстрой связи при подтверждении заявки.\n",
                            ParseMode.Html,
                            cancellationToken: ct);

                        _stateManager.SetUserState(userId, BotState.AwaitingContactInfo);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки документа от пользователя {UserId}", userId);

                await botClient.SendMessage(
                    message.Chat.Id,
                    "❌ Произошла ошибка при обработке документа. Пожалуйста, попробуйте еще раз.",
                    cancellationToken: ct);
            }
        }

        private async Task HandleContactInfoInput(ITelegramBotClient botClient, Message message, CancellationToken ct)
        {
            long userId = message.From.Id;
            BotSession session = SessionManager.GetORCreateSession(userId);
            string? requestId = session?.GetSessionData<string>("CurrentBookingRequestId");

            try
            {
                bool contactSaved = await _bookingService.AddContactInfo(requestId, message.Text);

                if (contactSaved)
                {
                    BookingRequest? request = await _bookingService.GetRequest(requestId);
                    await botClient.SendMessage(
                        message.Chat.Id,
                        "🎉 <b>Спасибо! Заявка полностью оформлена.</b>\n\n" +
                        "✅ Все документы получены\n" +
                        "✅ Контактная информация сохранена\n\n" +
                        "Ваша заявка передана на проверку администратору.\n" +
                        "Мы свяжемся с вами в ближайшее время для подтверждения бронирования.\n\n" +
                        "📞 <b>Наши контакты:</b>\n" +
                        "Телефон: +7 (963) 565-28-17\n\n" +
                        "🆔 <b>Номер вашей заявки:</b> " + requestId,
                        ParseMode.Html,
                        cancellationToken: ct);

                    // Уведомляем администраторов
                    await _adminService.NotifyNewBookingRequest(request);
                    _stateManager.SetUserState(userId, BotState.AdminReplyingToUser);
                }
                else
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        "❌ Не удалось сохранить контактную информацию. Попробуйте еще раз.",
                        cancellationToken: ct);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения контактной информации для пользователя {UserId}", userId);
            }

            // Очищаем сессию и состояние
            //SessionManager._sessions.Remove(userId);
            //_stateManager.SetUserState(userId, BotState.Default);
        }
    }
}
