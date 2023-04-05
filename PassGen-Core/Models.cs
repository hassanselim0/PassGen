using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PassGenCore;

public class KeyList
{
    public const int CurrVersion = 2;

    public int Version { get; set; } = CurrVersion;

    public MasterPassword Master { get; } = new MasterPassword();

    public ObservableCollection<Key> Keys { get; } = new();
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
