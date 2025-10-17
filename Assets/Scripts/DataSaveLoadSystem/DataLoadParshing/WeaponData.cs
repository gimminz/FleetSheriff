using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class WeaponData
{
    public int Id { get; set; }
    public string WeaponKey { get; set; }
    public string WeaponAddress { get; set; }
    public string DisplayName { get; set; }
    public int Category { get; set; }
    public int? AmmoProjectile { get; set; }      
    public float? Effect1 { get; set; }          
    public float? Effect2 { get; set; }           
    public float? Effect3 { get; set; }         
    public float? RateOfFire { get; set; }       
    public float? MaxRange { get; set; }        
    public int? AmmoCapacity { get; set; }     
    public int? NumberOfUses { get; set; }    
    public float? UseCooltime { get; set; }
    public int Hardpoint { get; set; }
    public int? UnlockLv { get; set; }
    public string UnlockPrecondition { get; set; }
    public int? Cost { get; set; }
    public string ItemDescription { get; set; }

    public override string ToString() => $"{Id} / {DisplayName} / HPt:{Hardpoint} / Addr:{WeaponAddress}";
}
public class WeaponTable : DataTable
{
    private readonly Dictionary<int, WeaponData> byId = new();
    private readonly List<WeaponData> cache = new();

    public override void Load(string filename)
    {
        byId.Clear();
        cache.Clear();

        var path = string.Format(FormatPath, filename);
        var ta = Resources.Load<TextAsset>(path);

        using (var reader = new StringReader(ta.text))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var d = new WeaponData
                {
                    Id = GetInt(csv, 0),
                    WeaponKey = GetStr(csv, 1),
                    WeaponAddress = GetStr(csv, 2),
                    DisplayName = GetStr(csv, 3),
                    Category = GetInt(csv, 4),
                    AmmoProjectile = GetNullableInt(csv, 5),
                    Effect1 = GetNullableFloat(csv, 6),
                    Effect2 = GetNullableFloat(csv, 7),
                    Effect3 = GetNullableFloat(csv, 8),
                    RateOfFire = GetNullableFloat(csv, 9),
                    MaxRange = GetNullableFloat(csv, 10),
                    AmmoCapacity = GetNullableInt(csv, 11),
                    NumberOfUses = GetNullableInt(csv, 12),
                    UseCooltime = GetNullableFloat(csv, 13),
                    Hardpoint = GetInt(csv, 14),
                    UnlockLv = GetNullableInt(csv, 15),
                    UnlockPrecondition = GetStr(csv, 16),
                    Cost = GetNullableInt(csv, 17),
                    ItemDescription = GetStr(csv, 18),
                };

                if (string.IsNullOrWhiteSpace(d.DisplayName))
                    d.DisplayName = d.WeaponKey ?? $"Weapon_{d.Id}";

                if (!byId.ContainsKey(d.Id))
                {
                    byId.Add(d.Id, d);
                    cache.Add(d);
                }
            }
        }
    }

    public WeaponData Get(int id)
    {
        return byId.TryGetValue(id, out var data) ? data : null; 
    }

    public bool Contains(int id) => byId.ContainsKey(id);

    public List<WeaponData> GetMany(IEnumerable<int> ids)
    {
        if (ids == null) return new List<WeaponData>();
        var list = new List<WeaponData>();
        foreach (var id in ids)
            if (byId.TryGetValue(id, out var d) && d != null) list.Add(d);
        return list;
    }

    public List<WeaponData> GetByCategoryAndHardpoint(int category, int hardpoint, bool ascending = true, string locale = "ko-KR")
    {
        var comp = System.StringComparer.Create(new CultureInfo(locale), true);
        var q = cache.Where(w => w.Category == category && w.Hardpoint == hardpoint);
        q = ascending ? q.OrderBy(w => w.DisplayName, comp) : q.OrderByDescending(w => w.DisplayName, comp);
        return q.ToList();
    }
    private static string GetStr(CsvReader csv, int index)
        => csv.TryGetField(index, out string s) ? s?.Trim() : null;

    private static int GetInt(CsvReader csv, int index, int def = 0)
    {
        var s = GetStr(csv, index);
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;
    }

    private static int? GetNullableInt(CsvReader csv, int index)
    {
        var s = GetStr(csv, index);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : (int?)null;
    }
    private static float? GetNullableFloat(CsvReader csv, int index)
    {
        var s = GetStr(csv, index);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v) ? v : (float?)null;
    }
}