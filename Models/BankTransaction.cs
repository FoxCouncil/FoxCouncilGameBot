// Copyright (c) 2018 The Fox Council

using LiteDB;

namespace FCGameBot.Models
{
    class BankTransaction
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        public long Debit { get; set; }

        public long Credit { get; set; }

        public string Memo { get; set; }
    }
}
