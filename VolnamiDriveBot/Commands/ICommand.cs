using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VolnamiDriveBot.Services.Commands;

namespace VolnamiDriveBot.Commands
{
    public interface ICommand
    {
        Task Execute(Message message, ITelegramBotClient botClient);
    }
}
