// Copyright (c) 2018 The Fox Council

using LiteDB;

namespace FCGameBot.Models
{
    class BankAccount
    {
        public long Id { get; set; }

        public int OwnerId { get; set; }

        public string Name { get; set; }

        public long Balance { get; set; }
    }
}
