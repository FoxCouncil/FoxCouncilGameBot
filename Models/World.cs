// Copyright (c) 2018 The Fox Council

using System;
using System.Threading.Tasks;
using LiteDB;
using Telegram.Bot.Types;

namespace FCGameBot.Models
{
    public class World : IEquatable<World>
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public bool Authorized { get; set; }

        [BsonIgnore]
        public Chat Chat { get; set; }

        public World()
        {

        }

        public World(Chat chat)
        {
            Chat = chat;

            Id = Chat.Id;
            Title = Chat.Title ?? Chat.Type.ToString();
            Type = Chat.Type.ToString();
        }

        public async Task SendMessage(string msg, Message reply = null)
        {
            if (reply is null)
            {
                await Game.SendMessage(Id, msg);
            }
            else
            {
                await Game.SendMessage(Id, msg, reply.MessageId);
            }
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