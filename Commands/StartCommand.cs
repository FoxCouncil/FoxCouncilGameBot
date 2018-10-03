using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class StartCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = false;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = false;

        public string[] Names { get; } = { "start" };

        public Task Help(Queue<string> args, Player player)
        {
            return null;
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {

            await player.SendMessage("Hi!");
        }

        public Task Callback(string data, Message msg, Player player)
        {
            return null;
        }
    }
}
