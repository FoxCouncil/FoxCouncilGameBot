// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;

namespace FCGameBot.Commands
{
    class WorldCommand : ICommand
    {
        private const string ActionAuth = "toggle_auth";

        private const string ActionCancel = "cancel";

        private const int WorldNameLength = 28;

        public bool Private { get; } = true;

        public bool Public { get; } = false;

        public bool Admin { get; } = true;

        public bool Targetable { get; } = false;

        public string[] Names { get; } = {"world", "worlds"};

        public async Task Help(Queue<string> args, Player player)
        {
            await player.SendMessage("Nothing...");
        }

        public async Task Process(string alias, Queue<string> args, Status player, Status targetPlayer = null)
        {
            var output = GenerateWorldList();

            await this.SendInlineKeyboard(player, output.Item1.ToString(), output.Item2);
        }

        public async Task Callback(string data, Message msg, Player player)
        {
            if (data.StartsWith(ActionAuth) && long.TryParse(data.Replace(ActionAuth, string.Empty), NumberStyles.Integer, null, out var chatId))
            {
                await ToggleAuth(chatId);

                var output = GenerateWorldList();

                await msg.EditMessage(output.Item1.ToString(), output.Item2);
            }
            else
            {
                await Game.RemoveMessage(msg);
            }
        }

        private static async Task ToggleAuth(long chatId)
        {
            var world = Game.Worlds.FindOne(x => x.Id == chatId);

            if (world == null)
            {
                return;
            }

            if (world.Authorized)
            {
                await Game.SendMessage(chatId, $"The Fox Council has closed the embassy here in {world.Title}, where the {world.Type.ToUpper()} ACT of 2001 specifies. Have a fluffy day.");
            }
            else
            {
                await Game.SendMessage(chatId, $"The Fox Council has opened an embassy here in {world.Title}, so general services are now available here where the {world.Type.ToUpper()} ACT of 2001 specifies. Have a fluffy day.");
            }

            world.Authorized = !world.Authorized;

            Game.Worlds.Update(world);
        }

        private static Tuple<StringBuilder, InlineKeyboard> GenerateWorldList()
        {
            var output = new StringBuilder();
            var keyboard = new InlineKeyboard();

            output.AppendLine($"**World Authorization Control** (Total: `{Game.Worlds.Count()}`)");

            foreach (var world in Game.Worlds.FindAll())
            {
                var title = world.Title;

                var subTitle = title.Substring(0, title.Length < WorldNameLength ? title.Length : WorldNameLength);

                output.AppendLine($"\t`{subTitle.PadRight(WorldNameLength)}` [[{(world.Authorized ? "✔" : "❌")}]]");

                keyboard.Add($"{ActionAuth}{world.Id}", $"{(world.Authorized ? "Deny" : "Auth")}: {subTitle}");
            }

            keyboard.Add(ActionCancel, "Done");

            return new Tuple<StringBuilder, InlineKeyboard>(output, keyboard);
        }
    }
}
