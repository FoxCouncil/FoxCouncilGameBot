using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using User = FCGameBot.Models.User;

namespace FCGameBot.Commands
{
    class StatusCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] GetNames() => new[] { "status" };

        public async Task Help(string alias, Queue<string> args, Status player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            await player.SendMessage($"Credits: {player.Credits.N("💳")}");
            await player.SendMessage($"Your language code is: {player.User.LanguageCode}");
        }
    }
}
