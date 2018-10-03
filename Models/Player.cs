// Copyright (c) 2018 The Fox Council

using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Telegram.Bot.Types;

namespace FCGameBot.Models
{
    internal class Player : IEquatable<Player>
    {
        public int Id { get; set; }

        public long SelectedWorld { get; set; }

        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string LanguageCode { get; set; }

        [BsonIgnore]
        public bool IsAdmin => Config.Data.Admins.Contains(Id);

        public Player()
        {

        }

        public Player(User telegramUser)
        {
            Id = telegramUser.Id;
            Firstname = telegramUser.FirstName;
            Lastname = telegramUser.LastName;
            Username = telegramUser.Username;
            LanguageCode = telegramUser.LanguageCode;
        }

        public async Task SendMessage(string msg)
        {
            await Game.SendMessage(Id, msg);
        }

        #region IEquatable<Player>

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Player)obj);
        }

        public bool Equals(Player other)
        {
            return other?.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(Player left, Player right)
        {
            return left?.Id == right?.Id;
        }
        public static bool operator !=(Player left, Player right)
        {
            return left?.Id != right?.Id;
        }

        public override string ToString()
        {
            return $"{Username} ({Id})";
        }

        #endregion
    }
}
