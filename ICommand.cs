// Copyright The Fox Council 2018

using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FCGameBot
{
    internal interface ICommand
    {
        bool Private { get; }

        bool Public { get; }

        bool Admin { get; }

        bool Targetable { get; }

        string[] GetNames();

        Task Help(string alias, Queue<string> args, Player player);

        Task Process(string alias, Queue<string> args, Chat chat, Player player, Player target = null);
    }
}
