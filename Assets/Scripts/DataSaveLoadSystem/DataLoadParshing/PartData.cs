using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class PartData
{
    public int Id { get; set; }
    public string ItemKey { get; set; }
    public string ItemAddress { get; set; }
    public string ItemDisplayName { get; set; }

    public int Category { get; set; }
    public int? Effect1 { get; set; }
    public int? Effect2 { get; set; }
    public int Hardpoint { get; set; }
    public int? UnlockLv { get; set; }
    public string UnlockPrecondition { get; set; }
    public int? Cost { get; set; }
    public string ItemDescription { get; set; }

    public override string ToString() => $"{Id} / {ItemDisplayName} / HardPoint:{Hardpoint} / Addr:{ItemAddress}";
}

public class PartTable : DataTable
{
    private readonly Dictionary<int, PartData> byId = new();
    private readonly List<PartData> cache = new();

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
                var p = new PartData
                {
                    Id = GetInt(csv, 0),
                    ItemKey = GetStr(csv, 1),
                    ItemAddress = GetStr(csv, 2),
                    ItemDisplayName = GetStr(csv, 3),
                    Category = GetInt(csv, 4),
                    Effect1 = GetNullableInt(csv, 5),
                    Effect2 = GetNullableInt(csv, 6),
                    Hardpoint = GetInt(csv, 7),
                    UnlockLv = GetNullableInt(csv, 8),
                    UnlockPrecondition = GetStr(csv, 9),
                    Cost = GetNullableInt(csv, 10),
                    ItemDescription = GetStr(csv, 11),
                };

                if (string.IsNullOrWhiteSpace(p.ItemDisplayName))
                    p.ItemDisplayName = p.ItemKey ?? $"Part_{p.Id}";

                if (!byId.ContainsKey(p.Id))
                {
                    byId.Add(p.Id, p);
                    cache.Add(p);
                }
                else
                {
                    Debug.LogError($"PartTable: duplicated ID {p.Id}");
                }
            }
        }
    }

    public PartData Get(int id) => byId.TryGetValue(id, out var d) ? d : null;
    public List<PartData> GetAll() => new(cache);

    public List<PartData> GetByHardpoint(int hardpoint, bool ascendingName = true, string locale = "ko-KR")
    {
        var comp = System.StringComparer.Create(new CultureInfo(locale), true);
        var q = cache.Where(p => p.Hardpoint == hardpoint);
        q = ascendingName ? q.OrderBy(p => p.ItemDisplayName, comp) : q.OrderByDescending(p => p.ItemDisplayName, comp);
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