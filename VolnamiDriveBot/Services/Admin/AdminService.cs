using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VolnamiDriveBot.Models.Domain;
using VolnamiDriveBot.Services.BookingService;
using VolnamiDriveBot.Services.Keyboard;

namespace VolnamiDriveBot.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly HashSet<long> _adminUserIds = new();
        private readonly ILogger<AdminService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITelegramBotClient _botClient;
        private readonly IKeyboardService _keyboardService;
        private readonly IBookingService _bookingService;

        public AdminService(IConfiguration configuration, ILogger<AdminService> logger, ITelegramBotClient botClient, IKeyboardService keyboardService, IBookingService bookingService)
        {
            _configuration = configuration;
            _botClient = botClient;
            _logger = logger;
            _keyboardService = keyboardService;
            _bookingService = bookingService;
            LoadAdminIds();
        }

        /// <summary>
        /// Загружает из конфигурационного файла все айди админов
        /// </summary>
        private void LoadAdminIds()
        {
            try
            {
                // Чтение из appsettings.json
                var adminIds = _configuration.GetSection("BotConfiguration:AdminIds").Get<long[]>();

                if (adminIds != null && adminIds.Length > 0)
                {
                    _adminUserIds.Clear();
                    foreach (var id in adminIds)
                    {
                        _adminUserIds.Add(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки ID администраторов");
            }
        }

        public bool IsAdmin(long userId) => _adminUserIds.Contains(userId);

        public async Task NotifyNewBookingRequest(BookingRequest request)
        {
            try
            {
                string contactInfo = request.TelegramUsername + " " + request.PhoneNumber;

                string message = $"🚨 <b>НОВАЯ ЗАЯВКА НА БРОНИРОВАНИЕ</b>\n\n" +
                       $"🆔 <b>ID заявки:</b> {request.Id}\n" +
                       $"👤 <b>Пользователь:</b> {request.UserName}\n" +
                       $"📞 <b>ID пользователя:</b> {request.UserId}\n\n" +
                       $"🔗 <b>Контакт:</b> {contactInfo}\n" +
                       $"🚗 <b>Транспорт:</b> {request.VehicleName}\n" +
                       $"📅 <b>Даты:</b> {request.StartDate:dd.MM.yyyy} - {request.EndDate:dd.MM.yyyy}\n" +
                       $"⏱️ <b>Продолжительность:</b> {request.TotalDays} суток\n\n" +
                       $"💰 <b>Стоимость аренды:</b> {request.TotalPrice}₽\n" +
                       $"💎 <b>Залог:</b> {request.PawnPrice}₽\n\n" +
                       $"📊 <b>Итого к оплате:</b> <u>{request.TotalPrice + request.PawnPrice}₽</u>\n\n" +
                       $"⏰ <b>Создана:</b> {request.CreatedAt:dd.MM.yyyy HH:mm}";

                foreach (long adminId in _adminUserIds)
                {
                    try
                    {
                        // Отправляем текстовое сообщение
                        await _botClient.SendMessage(
                            adminId,
                            message,
                            ParseMode.Html,
                            replyMarkup: _keyboardService.AdminAnswer(request));

                        // Отправляем фото документов если есть
                        await SendDocumentPhotos(adminId, request);

                        _logger.LogDebug("Уведомление о заявке {RequestId} отправлено админу {AdminId}",
                            request.Id, adminId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка отправки уведомления админу {AdminId}", adminId);
                    }
                }

                _logger.LogInformation("Уведомления о новой заявке {RequestId} отправлены всем администраторам",
                    request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки уведомлений о новой заявке");
            }
        }


        /// <summary>
        /// отправляет админу фото/файлы паспорта, ву
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task SendDocumentPhotos(long adminId, BookingRequest request)
        {
            // Отправляем фото паспорта
            if (!string.IsNullOrEmpty(request.PassportMainPhotoFileId))
            {
                try
                {
                    await _botClient.SendPhoto(
                        adminId,
                        InputFile.FromFileId(request.PassportMainPhotoFileId),
                        caption: "📄 Паспорт");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить фото паспорта админу");
                }
            }

            if (!string.IsNullOrEmpty(request.PassportRegistrationPhotoFileId))
            {
                try
                {
                    await _botClient.SendPhoto(
                        adminId,
                        InputFile.FromFileId(request.PassportRegistrationPhotoFileId),
                        caption: "📄 Паспорт");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить фото паспорта админу");
                }
            }

            // Отправляем фото ВУ
            if (!string.IsNullOrEmpty(request.DrivingLicenseFrontPhotoFileId))
            {
                try
                {
                    await _botClient.SendPhoto(
                        adminId,
                        InputFile.FromFileId(request.DrivingLicenseFrontPhotoFileId),
                        caption: "🪪 Водительское удостоверение");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить фото ВУ админу");
                }
            }

            if (!string.IsNullOrEmpty(request.DrivingLicenseBackPhotoFileId))
            {
                try
                {
                    await _botClient.SendPhoto(
                        adminId,
                        InputFile.FromFileId(request.DrivingLicenseBackPhotoFileId),
                        caption: "🪪 Водительское удостоверение");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось отправить фото ВУ админу");
                }
            }
        }

        public async Task AdminReplyToUser(ITelegramBotClient botClient, string requestId, Message message, CancellationToken ct)
        {
            long adminId = message.From!.Id;
            var bookingRequest = await _bookingService.GetRequest(requestId);

            if (bookingRequest != null)
            {
                try
                {
                    await botClient.SendMessage(
                        bookingRequest.UserId,
                        $"📨 <b>Сообщение от администратора VolnamiDrive</b>\n\n" +
                        $"{message.Text}\n\n",
                        ParseMode.Html);

                    await botClient.SendMessage(
                        adminId,
                        $"✅ Сообщение отправлено пользователю {bookingRequest.UserName}",
                        cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Не удалось отправить сообщение пользователю {UserId}", bookingRequest.UserId);

                    await botClient.SendMessage(
                        adminId,
                        $"❌ Не удалось отправить сообщение.\n",
                        cancellationToken: ct);
                }
            }
        }

        public async Task NotifyNewMessage(long userId, string userName, string messageText)
        {
            try
            {
                foreach (long adminId in _adminUserIds)
                {
                    try
                    {
                        await _botClient.SendMessage(
                            adminId,
                            $"💭 <b>НОВОЕ ПОЖЕЛАНИЕ ОТ ПОЛЬЗОВАТЕЛЯ</b>\n\n" +
                            $"👤 <b>Пользователь:</b> {userName}\n" +
                            $"🆔 <b>User ID:</b> {userId}\n\n" +
                            $"📝 <b>Сообщение:</b> {messageText}\n\n" +
                            $"🕐 <b>Получено:</b> {DateTime.Now:dd.MM.yyyy HH:mm}",
                            ParseMode.Html,
                            disableNotification: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка отправки пожелания админу {AdminId}", adminId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в NotifyNewMessage");
            }
        }
    }
}
