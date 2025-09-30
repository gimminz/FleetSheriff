using UnityEngine;
using System.Collections.Generic;

public class PlayerStatusData
{
    public int Level { get; set; }
    public int EXP { get; set; }
    public int Gold { get; set; }

    public override string ToString() => $"Lv {Level} / EXP {EXP} / Gold {Gold}";
}

public class PlayerStatusTable : DataTable
{
    public PlayerStatusData data;
    public override void Load(string filename)
    {
        data = null;

        var path = string.Format(FormatPath, filename);

        var textAsset = Resources.Load<TextAsset>(path); //fileLoad

        //debug
        if (textAsset == null)
        {
            Debug.LogError($"PlayerStatusTable: CSV not found at Resources/{path}.csv");
            return;
        }

        var list = LoadCSV<PlayerStatusData>(textAsset.text); //pasring

        //debug
        if (list == null || list.Count == 0)
        {
            Debug.LogError("PlayerStatusTable: CSV has no rows");
            return;
        }

        data = list[0];
    }
}
