using System;
using System.Collections.Generic;

namespace PassGenCore
{
    public class KeyList
    {
        public int Version { get; set; } = 1;

        public MasterPassword Master { get; } = new MasterPassword();

        public List<Key> Keys { get; } = new List<Key>();
    }

    public class MasterPassword
    {
        public string Hash { get; set; }

        public string Salt { get; set; }

        public int IterCount { get; set; } = 1000;
    }

    public class Key
    {
        public string Label { get; set; }

        public GenMode GenMode { get; set; }

        public int? MaxLength { get; set; }
    }

    public enum GenMode
    {
        Base64,
        AlphaNum,
    }
}