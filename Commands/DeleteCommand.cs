﻿// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class DeleteCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = false;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = true;

        public string[] Names { get; } = { "delete", "del", "rm" };

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            if (args.TryDequeue(out var action))
            {
                if (action.ToLower() == "player")
                {
                    if (targetPlayer == null)
                    {
                        await player.SendMessage("Sorry, you need to target a player to delete!");
                    }
                    else
                    {
                        var removed = Game.Users.Delete(x => x.Id == player.Player.Id);
                        await player.SendMessage(removed > 0
                            ? $"Player {player.Player.Username} was deleted successfully!"
                            : $"Could not delete player {player.Player.Username}!");

                        return;
                    }
                }
            }

            await player.SendMessage("Invalid syntax.");
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            return;
        }
    }
}
