using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    internal class PingCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = false;

        public string[] GetNames() => new[] {"ping"};

        public void Process(string alias, Queue<string> args, Chat chat, Player player)
        {
            chat.Reply("Pong!");
        }
    }
}
