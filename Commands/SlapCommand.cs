using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class SlapCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] GetNames() => new[] { "slap", "trout", "fishslap", "fish" };

        public Task Help(string alias, Queue<string> args, Player player)
        {
            throw new NotImplementedException();
        }

        public async Task Process(string alias, Queue<string> args, Chat chat, Player player, Player target = null)
        {
            if (player != null && player == target)
            {
                await chat.Reply($"`* {player.Username} slaps themselves in the face with a large trout.`");
            }
            else if (player != null && target != null)
            {
                await chat.Reply($"`* {player.Username} slaps {target.Username} around a bit with a large trout.`");
            }
        }
    }
}
