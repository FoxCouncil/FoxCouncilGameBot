// Copyright The Fox Council 2018

using System.Collections.Generic;
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

        void Process(string alias, Queue<string> args, Chat chat, Player player);
    }
}
