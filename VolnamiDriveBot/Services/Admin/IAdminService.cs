using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VolnamiDriveBot.Models.Domain;

namespace VolnamiDriveBot.Services.Admin
{
    public interface IAdminService
    {
        /// <summary>
        /// Проверяет, соответствует ли id пользователя id админа
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool IsAdmin(long  userId);

        /// <summary>
        /// отправляет админу новое бронирование, с приложением полученных фото/докукментов
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task NotifyNewBookingRequest(BookingRequest request);

        /// <summary>
        /// Отправляет ответ пользователю и уведомление админу об отправке
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="requestId"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AdminReplyToUser(ITelegramBotClient botClient, string requestId, Message message, CancellationToken ct);

        /// <summary>
        /// Отправляет админу сообщение о пожеланиях
        /// </summary>
        /// <param name="messageText"></param>
        /// <returns></returns>
        Task NotifyNewMessage(long userId, string userName, string messageText);


    }
}
