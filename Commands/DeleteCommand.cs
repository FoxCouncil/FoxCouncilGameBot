using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class DeleteCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = false;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = true;

        public string[] GetNames() => new[] { "delete", "del", "rm" };

        public async Task Help(string alias, Queue<string> args, Player player)
        {
            throw new NotImplementedException();
        }

        public async Task Process(string alias, Queue<string> args, Chat chat, Player player, Player target = null)
        {
            if (args.TryDequeue(out var action))
            {
                if (action.ToLower() == "player")
                {
                    if (target == null)
                    {
                        await player.SendMessage("Sorry, you need to target a player to delete!");
                    }
                    else
                    {
                        var removed = Game.Players.Delete(x => x.Id == target.Id);
                        await player.SendMessage(removed > 0
                            ? $"Player {target.Username} was deleted successfully!"
                            : $"Could not delete player {target.Username}!");

                        return;
                    }
                }
            }

            await player.SendMessage("Invalid syntax.");
        }
    }
}
