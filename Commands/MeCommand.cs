using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using User = FCGameBot.Models.User;

namespace FCGameBot.Commands
{
    class MeCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = false;

        public string[] GetNames() => new [] { "me" };

        public Task Help(string alias, Queue<string> args, Status player)
        {
            throw new NotImplementedException();
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            await player.SendMessage($"``` * {player.Username} {string.Join(' ', args)}```");
        }
    }
}
