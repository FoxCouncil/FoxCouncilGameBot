// Copyright (c) 2018 The Fox Council

using System;
using System.Collections.Generic;
using System.Text;

namespace FCGameBot.Models
{
    internal class Food
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string NamePlural { get; set; }

        public bool IsHealthy { get; set; }

        public uint Value { get; set; }
    }
}
