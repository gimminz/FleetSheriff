using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class ShipData
{
    [Name("ID")] public int Id { get; set; }
    [Name("Ship_name")] public string ShipName { get; set; }
    [Name("Ship_address")] public string ItemAddress { get; set; }
    [Name("Ship_HP")] public int ShipHP { get; set; }
    [Name("Ship_Shield")] public int ShipShield { get; set; }
    [Name("Shield-regen")] public float ShieldRegen { get; set; }
    [Name("Ship_Max_Speed")] public float ShipMaxSpeed { get; set; }
    [Name("MAX_Search_range")] public float MaxSearchRange { get; set; }
    [Name("Max_targeting")] public int MaxTargeting { get; set; }
    [Name("Unlock_Lv.")] public int? UnlockLv { get; set; }
    [Name("Unlock_Precondition")] public string UnlockPrecondition { get; set; }
    [Name("Buy_Cost")] public int? BuyCost { get; set; }
    [Name("Ship_Description")] public string ShipDescription { get; set; }

    public override string ToString() => $"{Id} / {ShipName} / HP:{ShipHP} / Shield:{ShipShield} / Regen:{ShieldRegen} / Speed:{ShipMaxSpeed}";
}

public class ShipTable : DataTable
{
    private readonly Dictionary<int, ShipData> table = new Dictionary<int, ShipData>();
    private readonly List<ShipData> cacheList = new List<ShipData>();

    public override void Load(string filename)
    {
        table.Clear();
        cacheList.Clear();

        var path = string.Format(FormatPath, filename); 
        var textAsset = Resources.Load<TextAsset>(path);
        var list = LoadCSV<ShipData>(textAsset.text);

        foreach (var ship in list)
        {
            if (ship == null) continue;

            if (string.IsNullOrWhiteSpace(ship.ShipName)) ship.ShipName = $"Ship_{ship.Id}";

            if (!table.ContainsKey(ship.Id))
            {
                table.Add(ship.Id, ship);
                cacheList.Add(ship);
            }
        }
    }

    public ShipData Get(int id)
    {
        return table.TryGetValue(id, out var data) ? data : null;
    }

    public List<ShipData> GetAll()
    {
        return new List<ShipData>(cacheList);
    }

    public List<ShipData> GetAllSortedByName(bool ascending = true, string locale = "ko-KR")
    {
        var comparer = StringComparer.Create(new CultureInfo(locale), ignoreCase: true);
        var ordered = ascending
            ? cacheList.OrderBy(s => s.ShipName, comparer)
            : cacheList.OrderByDescending(s => s.ShipName, comparer);
        return ordered.ToList();
    }

    public List<ShipData> FilterUnlockedByLevel(int playerLevel, bool includeNoRequirement = true)
    {
        return cacheList.Where(s =>
            (includeNoRequirement && s.UnlockLv == null) ||
            (s.UnlockLv.HasValue && s.UnlockLv.Value <= playerLevel)).ToList();
    }

    public List<ShipData> FilterUnlockedSortedByName(int playerLevel, bool ascending = true, string locale = "ko-KR")
    {
        var comparer = StringComparer.Create(new CultureInfo(locale), ignoreCase: true);
        var unlocked = FilterUnlockedByLevel(playerLevel, includeNoRequirement: true);
        return ascending
            ? unlocked.OrderBy(s => s.ShipName, comparer).ToList()
            : unlocked.OrderByDescending(s => s.ShipName, comparer).ToList();
    }

    public ShipData GetRandom()
    {
        if (cacheList.Count == 0) return null;
        return cacheList[UnityEngine.Random.Range(0, cacheList.Count)];
    }
}