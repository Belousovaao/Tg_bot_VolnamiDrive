using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolnamiDriveBot.Configuration
{
    public class BotConfiguration
    {
        public string Token { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public long[] AdminIds { get; set; } = Array.Empty<long>();
        public bool UseWebhook { get; set; }
        public string BotName { get; set; } = string.Empty;
    }
}
