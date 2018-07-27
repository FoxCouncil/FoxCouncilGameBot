using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using User = FCGameBot.Models.User;

namespace FCGameBot.Commands
{
    class DeleteCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = false;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = true;

        public string[] GetNames() => new[] { "delete", "del", "rm" };

        public async Task Help(string alias, Queue<string> args, Status player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            if (args.TryDequeue(out var action))
            {
                if (action.ToLower() == "user")
                {
                    if (targetPlayer == null)
                    {
                        await player.SendMessage("Sorry, you need to target a user to delete!");
                    }
                    else
                    {
                        var removed = Game.Users.Delete(x => x.Id == player.User.Id);
                        await player.SendMessage(removed > 0
                            ? $"User {player.User.Username} was deleted successfully!"
                            : $"Could not delete user {player.User.Username}!");

                        return;
                    }
                }
            }

            await player.SendMessage("Invalid syntax.");
        }
    }
}
