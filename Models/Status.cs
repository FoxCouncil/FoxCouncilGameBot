// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FCGameBot.Models
{
    internal class Status : IEquatable<Status>
    {
        public long Id { get; set; }

        public Player Player { get; set; }

        public World World { get; set; }

        // Helpers
        [BsonIgnore] public Message Message { get; set; }
        [BsonIgnore] public object Username => Player.Username;
        [BsonIgnore] public object Firstname => Player.Firstname;
        [BsonIgnore] public object Lastname => Player.Lastname;

        // Data
        public ushort Actions { get; set; }

        public DateTime LastAction { get; set; }

        public ulong Experience { get; set; }

        public long Credits { get; set; }

        public ulong Health { get; set; }

        public ulong Weight { get; set; }

        public Status()
        {

        }

        public Status(Player player, World world)
        {
            Player = player;
            World = world;

            if (World.Type != ChatType.Private.ToString())
            {
                // TODO: Defaults from the world

                Id = Game.Statuses.Insert(this);
            }
        }

        public async Task SendMessage(string msg)
        {
            if (World.Chat.Type == ChatType.Private)
            {
                await Player.SendMessage(msg);
            }
            else
            {
                await World.SendMessage(msg);
            }
        }

        public void Save()
        {
            Game.Statuses.Upsert(this);
        }

        public bool CanConsumeAction()
        {
            if (Actions == 0)
            {
                return false;
            }

            Actions--;

            LastAction = DateTime.Now;

            return true;
        }

        public override string ToString()
        {
            return $"[{Username}({Player.Id}) in \"{World.Type}: {World.Title}({World.Id})\"]({Id})";
        }

        #region IEquatable<Status>

        public bool Equals(Status other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Player, other.Player) && Equals(World, other.World);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Status) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable all NonReadonlyMemberInGetHashCode
                return ((Player != null ? Player.GetHashCode() : 0) * 397) ^ (World != null ? World.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
