using System;
using System.Collections.Generic;
using System.Text;

namespace FCGameBot
{
    internal class ConfigData
    {
        public string DatabaseFilename { get; set; } = "FCGameBot.db";

        public string TelegramBotApiKey { get; set; } = string.Empty;

        public int[] Admins { get; set; } = {};

        public float XpGrowthRatio { get; set; } = .3161f;
    }
}
