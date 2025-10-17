using System.Collections.Generic;
using System.Linq;

public class NpcData
{
    public int Id { get; set; }                
    public string NpcName { get; set; }         
    public string NpcAddress { get; set; }   
    public string KoreanName { get; set; }    
    public int Category { get; set; }           
    public int HP { get; set; }                 
    public int? Shield { get; set; }           
    public float? ShieldRegen { get; set; }    
    public float MaxSpeed { get; set; }     
    public int? Neutralize { get; set; }      
    public int? ExpReward1 { get; set; }    
    public int? CreditReward1 { get; set; }    
    public int? ExpReward2 { get; set; }  
    public int? CreditReward2 { get; set; }    
    public int? Weapon1 { get; set; }       
    public int? Weapon2 { get; set; }         

    public string DisplayName => string.IsNullOrWhiteSpace(KoreanName) ? NpcName : KoreanName;

    public override string ToString()
        => $"{Id} / {DisplayName} / HP:{HP} / Shield:{Shield} / Speed:{MaxSpeed}";
}

public class NpcTable : HeaderCsvTable<NpcData>
{
    protected override int GetId(NpcData row) => row.Id;

    protected override NpcData MapRow(IRowReader r, string locale)
    {
        return new NpcData
        {
            Id = r.GetInt("id"),
            NpcName = r.GetString("npc_name"),
            NpcAddress = r.GetString("npc_address"),
            KoreanName = r.GetString("korean_name"),

            Category = r.GetInt("category"),
            HP = r.GetInt("hp"),
            Shield = r.GetNullableInt("shield"),
            ShieldRegen = r.GetNullableFloat("shield_regen"),
            MaxSpeed = r.GetFloat("max_speed"),
            Neutralize = r.GetNullableInt("neutralize"),

            ExpReward1 = r.GetNullableInt("exp_reward_1"),
            CreditReward1 = r.GetNullableInt("credit_reward_1"),
            ExpReward2 = r.GetNullableInt("exp_reward_2"),
            CreditReward2 = r.GetNullableInt("credit_reward_2"),

            Weapon1 = r.GetNullableInt("weapon_1"),
            Weapon2 = r.GetNullableInt("weapon_2"),
        };
    }
    public NpcData Get(int id) => TryGetById(id, out var d) ? d : null;
    public List<NpcData> GetAll() => new List<NpcData>(All);
    public List<NpcData> GetByCategory(int category) => All.Where(n => n.Category == category).ToList();
}

