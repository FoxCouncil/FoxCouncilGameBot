// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class SlapCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] Names { get; } = { "slap", "trout", "fishslap", "fish" };

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
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

        public async Task Callback(string data, Message msg, Player player)
        {
            return;
        }
    }
}
