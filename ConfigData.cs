using System;
using System.Collections.Generic;
using System.Text;

namespace FCGameBot
{
    internal class ConfigData
    {
        public string TelegramBotApiKey { get; set; } = string.Empty;

        public int[] Admins { get; set; } = {};
    }
}
