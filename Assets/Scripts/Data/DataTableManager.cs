using System.Collections.Generic;
using UnityEngine;

public class DataTableManager : MonoBehaviour
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

    static DataTableManager()
    {
        Init();
    }

    private static void Init()
    {
        var playerStatusTable = new PlayerStatusTable();
        playerStatusTable.Load(DataTableIds.PlayerStatus);
        tables.Add(DataTableIds.PlayerStatus, playerStatusTable);

        var shipTable = new ShipTable();
        shipTable.Load(DataTableIds.Ship);
        tables.Add(DataTableIds.Ship, shipTable);

        var part = new PartTable();
        part.Load(DataTableIds.Part);
        tables[DataTableIds.Part] = part;

        var weap = new WeaponTable();
        weap.Load(DataTableIds.Weapon);
        tables[DataTableIds.Weapon] = weap;
    }

    public static PlayerStatusTable PlayerStatusTable
    {
        get { return Get<PlayerStatusTable>(DataTableIds.PlayerStatus); }
    }

    public static ShipTable ShipTable
    {
        get { return Get<ShipTable>(DataTableIds.Ship); }
    }
    public static PartTable PartTable
    {
        get { return Get<PartTable>(DataTableIds.Part); }
    }

    public static WeaponTable WeaponTable
    {
        get { return Get<WeaponTable>(DataTableIds.Weapon); }
    }

    public static T Get<T>(string id) where T : DataTable
    {   
        //debug
        if (!tables.ContainsKey(id))
        {
            Debug.LogError($"테이블 없음: {id}");
            return null;
        }

        return tables[id] as T;
    }
}
