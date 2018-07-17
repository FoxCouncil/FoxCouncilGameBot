using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace FCGameBot
{
    internal static class Extensions
    {
        public static void Reply(this Chat chat, string msg)
        {
            Game.SendMessage(chat.Id, msg);
        }
    }
}
