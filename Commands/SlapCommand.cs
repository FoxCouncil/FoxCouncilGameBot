using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using User = FCGameBot.Models.User;

namespace FCGameBot.Commands
{
    class SlapCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] GetNames() => new[] { "slap", "trout", "fishslap", "fish" };

        public Task Help(string alias, Queue<string> args, Status player)
        {
            throw new NotImplementedException();
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            if (targetPlayer != null && player.Id == targetPlayer.Id)
            {
                await player.SendMessage($"`* {player.Username} slaps themselves in the face with a large trout.`");
            }
            else if (player != null && targetPlayer != null)
            {
                await player.SendMessage($"`* {player.Username} slaps {targetPlayer.Username} around a bit with a large trout.`");
            }
        }
    }
}
