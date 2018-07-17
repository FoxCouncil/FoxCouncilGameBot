using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    internal class HelpCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = false;

        public string[] GetNames() => new[] { "help" };

        public void Process(string alias, Queue<string> args, Chat chat, Player player)
        {
            player.SendMessage("Here is some help; <help>");
        }
    }
}
