using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class LoadoutSaveModel
{
    [JsonProperty("version")] public int Version = 1;
    [JsonProperty("ships")] public Dictionary<int, ShipLoadout> Ships = new();
}

[Serializable]
public class ShipLoadout
{
    [JsonProperty("shipKey")] public int ShipKey;
    [JsonProperty("parts")] public Dictionary<ShipSlot, int?> Parts = new();
}
