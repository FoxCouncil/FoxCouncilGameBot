// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FCGameBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FCGameBot
{
    internal static class Extensions
    {
        public static async Task Remove(this Message msg)
        {
            await Game.RemoveMessage(msg);
        }

        public static async Task<Message> EditMessage(this Message msg, string newText, Dictionary<string, string> inlineKeyboard = null)
        {
            return await Game.EditMessage(msg, newText, inlineKeyboard);
        }

        public static async Task<Message> EditMessage(this Message msg, string newText)
        {
            return await Game.EditMessage(msg, newText);
        }

        public static async Task SendInlineKeyboard(this ICommand command, Status playerStatus, string message, Dictionary<string, string> keyboardButtons)
        {
            await Game.SendMessage(playerStatus.Player.Id, message, keyboardButtons, command);
        }

        public static async Task SendInlineKeyboard(this ICommand command, ChatId chatId, string message, Dictionary<string, string> keyboardButtons)
        {
            await Game.SendMessage(chatId, message, keyboardButtons, command);
        }

        public static T DequeueAt<T>(this List<T> list, int index)
        {
            var r = list[index];
            list.RemoveAt(index);
            return r;
        }

        public static T DequeueFirst<T>(this List<T> list, Predicate<T> predicate)
        {
            var index = list.FindIndex(predicate);
            var r = list[index];
            list.RemoveAt(index);
            return r;
        }

        public static string P(this string stringToPluralize)
        {
            if (stringToPluralize.Length <= 2)
            {
                return stringToPluralize;
            }

            return !stringToPluralize.EndsWith('s') ? $"{stringToPluralize}s" : stringToPluralize;
        }

        public static string N(this bool val)
        {
            return val ? "ON" : "OFF";
        }

        public static string N(this ulong val, string tag)
        {
            return $"*{string.Format("{0:n0}", val)}*`{tag.ToUpper()}`";
        }

        public static string N(this long val, string tag)
        {
            return $"*{string.Format("{0:n0}", val)}*`{tag.ToUpper()}`";
        }

        public static string M(this long val)
        {
            return $"`$`*{string.Format("{0:n2}", val / 100)}*";
        }

        public static string N(this float val, string tag)
        {
            return $"*{string.Format("{0:n0}", val)}*`{tag.ToUpper()}`";
        }
    }
}
