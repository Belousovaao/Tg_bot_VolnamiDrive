using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VolnamiDriveBot.Commands.Concrete
{
    public class HelpCommand : ICommand
    {
        private readonly ILogger<StartCommand> _logger;

        public HelpCommand(ILogger<StartCommand> logger)
        {
            _logger = logger;
        }

        public async Task Execute(Message message, ITelegramBotClient botClient)
        {
            try
            {
                string helpText = @"🚗 <b>Помощь по боту VolnamiDrive</b>

                *⚡ Быстрые команды*
                /start - Запустить бота
                /help - Эта справка
                /contact - Связаться с нами

                *📅 Бронирование*
                • Запустите бот /start
                • Выберите транспорт из каталога
                • Укажите даты в календаре
                • Получите рассчет стоимости
                • Свяжитесь с нами для подтверждения бронирования

                *❓ Частые вопросы*
                Q: Как подтвердить бронирорование?
                A: Свяжитесь с нами

                Q: Какие требования?
                A: • Права категории B для авто, A для мото;
                   • Возраст водителя 23+;
                   • Стаж вождения от 3-х лет;
                   • За каждое ТС обязательно нужно внести залог, сумма залога будет указана после расчета стоимости.

                Q: Транспортные средства застрахованы?
                A: Да, на каждое транспортное средство оформлено ОСАГО и КАСКО.

                *🆘 Поддержка*
                📞 Телефон: +7 (963) 565-28-17
                ✉️ Email: volodin_rent@mail.ru
                🕒 Время работы: 8:00 - 21:00

                Свяжитесь с нами, если не нашли ответ на свой вопрос!";

                await botClient.SendMessage(
                    chatId: message.From.Id,
                    text: helpText,
                    parseMode: ParseMode.Html);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка выполнения команды /help");

            }
        }

    }
}
