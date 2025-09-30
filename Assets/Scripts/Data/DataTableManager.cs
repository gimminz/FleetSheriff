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

        var effect = new EffectTable();         
        effect.Load(DataTableIds.Effect);
        tables[DataTableIds.Effect] = effect;

        var npc = new NpcTable();
        npc.Load(DataTableIds.Npc);
        tables[DataTableIds.Npc] = npc;

        var mission = new MissionTable();
        mission.Load(DataTableIds.Mission);
        tables[DataTableIds.Mission] = mission;

        var lv = new LvTable();
        lv.Load(DataTableIds.Lv);
        tables[DataTableIds.Lv] = lv;
    }

    public static PlayerStatusTable PlayerStatusTable => Get<PlayerStatusTable>(DataTableIds.PlayerStatus);
    public static ShipTable ShipTable => Get<ShipTable>(DataTableIds.Ship);
    public static PartTable PartTable => Get<PartTable>(DataTableIds.Part);
    public static WeaponTable WeaponTable => Get<WeaponTable>(DataTableIds.Weapon);
    public static EffectTable EffectTable => Get<EffectTable>(DataTableIds.Effect);
    public static NpcTable NpcTable => Get<NpcTable>(DataTableIds.Npc);
    public static MissionTable MissionTable => Get<MissionTable>(DataTableIds.Mission);
    public static LvTable LvTable => Get<LvTable>(DataTableIds.Lv);

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
