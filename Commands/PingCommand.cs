// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    internal class PingCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = false;

        public string[] Names { get; } = { "ping" };

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            await player.SendMessage("Pong!");
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            return;
        }
    }
}
