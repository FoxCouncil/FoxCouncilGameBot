using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace FCGameBot
{
    internal static class Extensions
    {
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

        public static string N(this float val, string tag)
        {
            return $"*{string.Format("{0:n0}", val)}*`{tag.ToUpper()}`";
        }

        public static async Task Reply(this Chat chat, string msg)
        {
            await Game.SendMessage(chat.Id, msg);
        }
    }
}
