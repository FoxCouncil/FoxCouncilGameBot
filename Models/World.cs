//   !!  // FCGameBot - World.cs
// *.-". // Created: 2018-07-25 [11:22 PM]
//  | |  // Copyright 2018 The Fox Council 
// Modified by: Fox Diller on 2018-07-25 [11:22 PM]

using System;
using LiteDB;
using Telegram.Bot.Types;

namespace FCGameBot.Models
{
    public class World : IEquatable<World>
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        [BsonIgnore]
        public Chat Chat { get; set; }

        public World()
        {

        }

        public World(Chat chat)
        {
            Chat = chat;

            Id = Chat.Id;
            Title = Chat.Title;
            Type = Chat.Type.ToString();
        }

        public override string ToString()
        {
            return $"{Type}: {Title}({Id})";
        }

        #region IEquatable<World>

        public bool Equals(World other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((World)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}