// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class StatusCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = false;

        public bool Targetable { get; } = true;

        public string[] Names { get; } = { "status" };

        public async Task Help(Queue<string> args, Player player)
        {
            await Game.SendMessage(player.Id, "Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            await player.SendMessage($"Credits: {player.Credits.N("💳")}");
            await player.SendMessage($"Your language code is: {player.Player.LanguageCode}");
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            return;
        }
    }
}
