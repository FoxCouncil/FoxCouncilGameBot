// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot
{
    internal interface ICommand
    {
        bool Private { get; }

        bool Public { get; }

        bool Admin { get; }

        bool Targetable { get; }

        string[] Names { get; }

        Task Help(Queue<string> args, Player player);

        Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null);

        Task Callback(string data, Message msg, Player player);
    }
}
