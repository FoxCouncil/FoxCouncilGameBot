// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class MeCommand : ICommand
    {
        public bool Private { get; } = false;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = false;

        public string[] Names { get; } = { "me", "emote" };

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            await player.SendMessage($"``` * {player.Username} {string.Join(' ', args)}```");
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            return;
        }
    }
}
