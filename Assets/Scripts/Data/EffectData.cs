public class EffectData
{
    public int Id { get; set; }           
    public string EffectKey { get; set; }     
    public string DisplayName { get; set; }   
    public int Category { get; set; }   
    public float Increase { get; set; }    
    public string Description { get; set; } 

    public override string ToString()
        => $"{Id} / {DisplayName} (+{Increase}%) / Cat:{Category}";
}
public class EffectTable : HeaderCsvTable<EffectData>
{
    protected override int GetId(EffectData row) => row.Id;

    protected override EffectData MapRow(IRowReader r, string locale)
    {
        return new EffectData
        {
            Id = r.GetInt("ID"),
            EffectKey = r.GetString("EFFECT_NAME"),
            DisplayName = r.GetFirstString("Korean_NAME", "EFFECT_NAME"),
            Category = r.GetInt("CATEGORY"),
            Increase = r.GetFloat("INCREASE_EFFECT"),
            Description = r.GetString("Description", "")
        };
    }
}
