// Services/Handlers/AdminCallbackHandler.cs
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Models.Enums;
using VolnamiDriveBot.Services.Admin;
using VolnamiDriveBot.Services.BookingService;
using VolnamiDriveBot.Services.Keyboard;
using VolnamiDriveBot.Services.StateManagement;

namespace VolnamiDriveBot.Services.Handlers
{
    public class AdminCallbackHandler
    {
        private readonly IUserStateManager _stateManager;
        private readonly IBookingService _bookingService;
        private readonly IAdminService _adminService;

        public AdminCallbackHandler(IUserStateManager stateManager, IBookingService bookingService, IAdminService adminService)
        {
            _stateManager = stateManager;
            _bookingService = bookingService;
            _adminService = adminService;
        }

        public async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
        {
            long adminId = callbackQuery.From.Id;
            string? callbackData = callbackQuery.Data;

            if (string.IsNullOrEmpty(callbackData))
                return;

            // Проверка прав администратора
            if (!_adminService.IsAdmin(adminId))
            {
                await botClient.AnswerCallbackQuery(
                    callbackQuery.Id,
                    "⛔ Доступ запрещен",
                    showAlert: true,
                    cancellationToken: ct);
                return;
            }

            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

            try
            {
                // Обработка админ-действий
                switch (callbackData)
                {
                    // Ответ пользователю
                    case string s when s.StartsWith("admin_reply_"):
                        await HandleAdminReplyRequest(botClient, callbackQuery, s, ct);
                        break;

                    case string s when s.StartsWith("admin_show_phone"):
                        await HandleAdminShowPhone(botClient, callbackQuery, ct);
                        break;
                }
            }
            catch (Exception ex)
            {       await botClient.SendMessage(
                    adminId,
                    "❌ Произошла ошибка при обработке запроса",
                    cancellationToken: ct);
            }
        }

        private async Task HandleAdminReplyRequest(ITelegramBotClient botClient, CallbackQuery callbackQuery, string callbackData, CancellationToken ct)
        {
            long adminId = callbackQuery.From.Id;
            var requestId = callbackData.Replace("admin_reply_", "");
            var bookingRequest = await _bookingService.GetRequest(requestId);
            UserState userState = _stateManager.GetUserState(adminId);

            if (bookingRequest == null)
            {
                await botClient.SendMessage(
                    adminId,
                    "❌ Заявка не найдена",
                    cancellationToken: ct);
                return;
            }

            // Сохраняем данные для ответа
            userState.SetData("replying_to", requestId);
            userState.SetData("reply_user_id", bookingRequest.UserId);
            _stateManager.SetUserState(adminId, BotState.AdminReplyingToUser);

            await botClient.SendMessage(
                adminId,
                $"💬 <b>Ответ пользователю</b>\n\n" +
                $"Заявка: #{bookingRequest.Id}\n" +
                $"Пользователь: {bookingRequest.UserName}\n" +
                $"UserID: {bookingRequest.UserId}\n" +
                $"Контакт: {bookingRequest.GetContactLink()}\n\n" +
                "<i>Введите ваш ответ:</i>",
                ParseMode.Html,
                cancellationToken: ct);
        }

        private async Task HandleAdminShowPhone(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
        {
            long adminId = callbackQuery.From.Id;
            var requestId = callbackQuery.Data.Replace("admin_show_phone_", "");

            var bookingRequest = await _bookingService.GetRequest(requestId);

            await botClient.SendMessage(
                adminId,
                $"📞 Телефон пользователя {bookingRequest.UserName}:\n" +
                $"{bookingRequest.PhoneNumber}");
        }
    }
}