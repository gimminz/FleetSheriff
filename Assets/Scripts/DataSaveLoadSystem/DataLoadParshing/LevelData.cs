using System.Collections.Generic;

public class LevelData
{
    public int Lv { get; set; }         
    public int NeedExp { get; set; }     

    public override string ToString() => $"Lv {Lv} ¡æ {NeedExp}";
}

public class LvTable : HeaderCsvTable<LevelData>
{
    protected override int GetId(LevelData row) => row.Lv;

    protected override LevelData MapRow(IRowReader r, string locale)
    {
        return new LevelData
        {
            Lv = r.GetInt("lv"),
            NeedExp = r.GetInt("need_exp")
        };
    }

    public LevelData Get(int lv) => TryGetById(lv, out var d) ? d : null;
    public IReadOnlyList<LevelData> AllLevels => All;
}