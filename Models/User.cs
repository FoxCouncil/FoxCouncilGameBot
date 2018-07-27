using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Telegram.Bot.Types;
using TelegramUser = Telegram.Bot.Types.User;

namespace FCGameBot.Models
{
    internal class User : IEquatable<User>
    {
        public int Id { get; set; }

        public long SelectedWorld { get; set; }

        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string LanguageCode { get; set; }

        [BsonIgnore]
        public bool IsAdmin => Config.Data.Admins.Contains(Id);

        public User()
        {

        }

        public User(TelegramUser telegramUser)
        {
            Id = telegramUser.Id;
            Firstname = telegramUser.FirstName;
            Lastname = telegramUser.LastName;
            Username = telegramUser.Username;
            LanguageCode = telegramUser.LanguageCode;
        }

        #region IEquatable<User>

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((User)obj);
        }

        public bool Equals(User other)
        {
            return other?.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(User left, User right)
        {
            return left?.Id == right?.Id;
        }
        public static bool operator !=(User left, User right)
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
