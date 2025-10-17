using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class LoadoutSaveModel
{
    [JsonProperty("version")] public int Version = 2;
    [JsonProperty("ships")] public Dictionary<int, ShipLoadout> Ships = new();
}

[Serializable]
public class ShipLoadout
{
    [JsonProperty("shipKey")] public int ShipKey;
    [JsonProperty("parts")] public Dictionary<ShipSlot, int?> Parts = new();
    [JsonProperty("weapons")] public Dictionary<ShipSlot, int?> Weapons = new();

    public ShipLoadout()
    {
        Parts ??= new();
        Weapons ??= new();
    }
}
