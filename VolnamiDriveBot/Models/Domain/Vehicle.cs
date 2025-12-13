using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace VolnamiDriveBot.Models.Domain
{
    public class Vehicle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        public string Type { get; set; }
        public int DailyPrice { get; set; }
        public int PawnPrice { get; set; }
        public string FullDescription { get; set; }

    }
}
