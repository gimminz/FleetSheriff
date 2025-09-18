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
        if (ta == null)
        {
            Debug.LogError($"WeaponTable: CSV not found at Resources/{path}.csv");
            return;
        }

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
                else
                {
                    Debug.LogError($"WeaponTable: duplicated ID {d.Id}");
                }
            }
        }
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
}
