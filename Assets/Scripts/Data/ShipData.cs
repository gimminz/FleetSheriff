using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
public class ShipData
{
    public int Id { get; set; }                 
    public string ShipName { get; set; }    
    public string KoreanName { get; set; }       
    public string ItemAddress { get; set; }    
    public int ShipHP { get; set; }             
    public float ShipShield { get; set; }       
    public float ShieldRegen { get; set; }     
    public float ShipMaxSpeed { get; set; }     
    public float MaxSearchRange { get; set; }    
    public int MaxTargeting { get; set; }     
    public int? UnlockLv { get; set; }           
    public string UnlockPrecondition { get; set; }
    public int? BuyCost { get; set; }        
    public string ShipDescription { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(KoreanName) ? ShipName : KoreanName;

    public override string ToString()
        => $"{Id} / {DisplayName} / HP:{ShipHP} / Shield:{ShipShield} / Regen:{ShieldRegen} / Speed:{ShipMaxSpeed}";
}

public class ShipTable : HeaderCsvTable<ShipData>
{
    protected override int GetId(ShipData row) => row.Id;

    protected override ShipData MapRow(IRowReader r, string locale)
    {
        var id = r.GetInt("id");
        var shipName = r.GetString("ship_name");
        var krName = r.GetString("korean_name");

        return new ShipData
        {
            Id = id,
            ShipName = string.IsNullOrWhiteSpace(shipName) ? $"Ship_{id}" : shipName,
            KoreanName = krName,
            ItemAddress = r.GetString("ship_address"),

            ShipHP = r.GetInt("ship_hp"),
            ShipShield = r.GetFloat("ship_shield"),
            ShieldRegen = r.GetFloat("shield_regen"),
            ShipMaxSpeed = r.GetFloat("ship_max_speed"),
            MaxSearchRange = r.GetFloat("max_search_range"),
            MaxTargeting = r.GetInt("max_targeting"),

            UnlockLv = r.GetNullableInt("unlock_lv"),
            UnlockPrecondition = r.GetString("unlock_precondition"),
            BuyCost = r.GetNullableInt("buy_cost"),
            ShipDescription = r.GetString("ship_description", "")
        };
    }
    public ShipData Get(int id) => TryGetById(id, out var d) ? d : null;

    public List<ShipData> GetAll() => new List<ShipData>(All);

    public List<ShipData> GetAllSortedByName(bool ascending = true, string locale = "ko-KR")
    {
        var comp = System.StringComparer.Create(new CultureInfo(locale), true);
        var q = ascending
            ? All.OrderBy(s => s.DisplayName, comp)
            : All.OrderByDescending(s => s.DisplayName, comp);
        return q.ToList();
    }

    public List<ShipData> FilterUnlockedByLevel(int playerLevel, bool includeNoRequirement = true)
    {
        return All.Where(s =>
            (includeNoRequirement && s.UnlockLv == null) ||
            (s.UnlockLv.HasValue && s.UnlockLv.Value <= playerLevel)).ToList();
    }

    public List<ShipData> FilterUnlockedSortedByName(int playerLevel, bool ascending = true, string locale = "ko-KR")
    {
        var comp = System.StringComparer.Create(new CultureInfo(locale), true);
        var unlocked = FilterUnlockedByLevel(playerLevel, true);
        return ascending
            ? unlocked.OrderBy(s => s.DisplayName, comp).ToList()
            : unlocked.OrderByDescending(s => s.DisplayName, comp).ToList();
    }

    public ShipData GetRandom()
    {
        if (All.Count == 0) return null;
        return All[UnityEngine.Random.Range(0, All.Count)];
    }
}