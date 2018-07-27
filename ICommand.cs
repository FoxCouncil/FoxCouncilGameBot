// Copyright The Fox Council 2018

using System.Collections.Generic;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using User = FCGameBot.Models.User;

namespace FCGameBot
{
    internal interface ICommand
    {
        bool Private { get; }

        bool Public { get; }

        bool Admin { get; }

        bool Targetable { get; }

        string[] GetNames();

        Task Help(string alias, Queue<string> args, Status player);

        Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null);
    }
}
