using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VolnamiDriveBot.Services.Commands;

namespace VolnamiDriveBot.Commands.Concrete
{
    public class ContactCommand : ICommand
    {
        private readonly ILogger<StartCommand> _logger;

        public ContactCommand(ILogger<StartCommand> logger)
        {
            _logger = logger;
        }
        public async Task Execute(Message message, ITelegramBotClient botClient)
        {
            try
            {
                string helpText = @"📞 <b>Как с нами связаться?</b>
                📞 Телефон / What'sApp: +7 (963) 565-28-17
                ✉️ Email: volodin_rent@mail.ru
                📱Telegram: @Anna_Volodina24
                🕒 Время работы: 8:00 - 21:00";

                await botClient.SendMessage(
                    chatId: message.From.Id,
                    text: helpText,
                    parseMode: ParseMode.Html);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка выполнения команды /contact");

            }
        }
    }
}
