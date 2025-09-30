using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class MissionNpcUse
{
    public int NpcId { get; set; }
    public int Count { get; set; }
}

public class MissionData
{
    public int Id { get; set; }                 
    public string MissionType { get; set; }     
    public string KoreanName { get; set; }   
    public int LimitTime { get; set; }         
    public int? UserCount { get; set; }         
    public int ExpReward { get; set; }       
    public int CreditReward { get; set; }      
    public List<MissionNpcUse> Npcs { get; } = new(); 
}

public class MissionTable : DataTable
{
    private readonly Dictionary<int, MissionData> byId = new();
    private readonly List<MissionData> cache = new();

    public MissionData Get(int id) => byId.TryGetValue(id, out var d) ? d : null;
    public IReadOnlyList<MissionData> All => cache;

    public override void Load(string filename)
    {
        byId.Clear();
        cache.Clear();

        var path = string.Format(FormatPath, filename);
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"MissionTable: CSV not found at Resources/{path}.csv");
            return;
        }

        using var reader = new StringReader(ta.text);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;
        if (headers == null || headers.Length == 0)
        {
            Debug.LogError("MissionTable: empty header");
            return;
        }

        var normMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            var h = headers[i]?.Trim();
            if (string.IsNullOrEmpty(h)) continue;
            var key = NormalizeKey(h);
            if (!normMap.ContainsKey(key)) normMap[key] = i;
        }

        int Idx(string name) => normMap.TryGetValue(NormalizeKey(name), out var i) ? i : -1;

        var idxId = Idx("ID");
        var idxType = Idx("mission_type");
        var idxKrName = Idx("Korean_name");
        var idxLimit = Idx("Limit_time");
        var idxUser = Idx("User");
        var idxExp = Idx("exp_reward");
        var idxCredit = Idx("credit_reward");

        if (idxId < 0 || idxType < 0 || idxKrName < 0 || idxLimit < 0 || idxExp < 0 || idxCredit < 0)
        {
            Debug.LogError("MissionTable: required headers missing (ID, mission_type, Korean_name, Limit_time, exp_reward, credit_reward)");
            return;
        }

        var npcIdx = new int[4];
        for (int k = 0; k < 4; k++)
        {
            npcIdx[k] = Idx($"Use_NPC {k + 1}");
        }

        while (csv.Read())
        {
            try
            {
                var d = new MissionData
                {
                    Id = SafeInt(csv, idxId),
                    MissionType = SafeStr(csv, idxType),
                    KoreanName = SafeStr(csv, idxKrName),
                    LimitTime = SafeInt(csv, idxLimit),
                    UserCount = idxUser >= 0 ? SafeNullableInt(csv, idxUser) : null,
                    ExpReward = SafeInt(csv, idxExp),
                    CreditReward = SafeInt(csv, idxCredit),
                };

                for (int k = 0; k < 4; k++)
                {
                    var idCol = npcIdx[k];
                    if (idCol < 0) continue;

                    var npcId = SafeNullableInt(csv, idCol);
                    if (npcId == null) continue;

                    int? count = null;

                    var countNamedIdx =
                        Idx($"Use_NPC {k + 1} Count");
                    if (countNamedIdx >= 0)
                    {
                        count = SafeNullableInt(csv, countNamedIdx);
                    }

                    if (count == null)
                    {
                        var next = idCol + 1;
                        if (next < headers.Length)
                            count = SafeNullableInt(csv, next);
                    }

                    d.Npcs.Add(new MissionNpcUse
                    {
                        NpcId = npcId.Value,
                        Count = count ?? 0
                    });
                }

                if (!byId.ContainsKey(d.Id))
                {
                    byId[d.Id] = d;
                    cache.Add(d);
                }
                else
                {
                    Debug.LogError($"MissionTable: duplicated ID {d.Id}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"MissionTable parse error at row {csv.Context?.Parser?.Row}: {ex.Message}");
            }
        }
    }

    private static string SafeStr(CsvReader csv, int index)
        => index >= 0 && csv.TryGetField(index, out string s) ? s?.Trim() : null;

    private static int SafeInt(CsvReader csv, int index, int def = 0)
    {
        var s = SafeStr(csv, index);
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;
    }

    private static int? SafeNullableInt(CsvReader csv, int index)
    {
        var s = SafeStr(csv, index);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : (int?)null;
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var chars = s.ToCharArray();
        var sb = new System.Text.StringBuilder(chars.Length);
        foreach (var ch in chars)
            sb.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '_');

        var norm = sb.ToString();
        while (norm.Contains("__")) norm = norm.Replace("__", "_");
        return norm.Trim('_');
    }
}
