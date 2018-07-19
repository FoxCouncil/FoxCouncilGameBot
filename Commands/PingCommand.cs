﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    internal class PingCommand : ICommand
    {
        public bool Private { get; } = true;

        public bool Public { get; } = true;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = false;

        public string[] GetNames() => new[] { "ping" };

        public async Task Help(string alias, Queue<string> args, Player player)
        {
            throw new NotImplementedException();
        }

        public async Task Process(string alias, Queue<string> args, Chat chat, Player player, Player target = null)
        {
            await chat.Reply("Pong!");
        }
    }
}
