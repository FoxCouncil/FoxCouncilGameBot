using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class FeedCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] Names { get; } = { "feed", "cook", "eat" };

        public Task Help(Queue<string> args, Player player)
        {
            return null;
        }

        public Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            return null;
        }

        public Task Callback(string data, Message msg, Player player)
        {
            return null;
        }
    }
}
