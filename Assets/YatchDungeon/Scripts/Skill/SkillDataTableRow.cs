using KCoreKit;

namespace YatchDungeon
{
    public enum SkillType
    {
        Basic,
        Active,
        Passive
    }
        
    public class SkillDataTableRow : DataTableRowBase
    {
        public SkillType skillType;
        public string nameKey;
        public string descKey;
        public string abilityId;
        public float cooldown;
        public float castTime;
        public UnitAttackTargetOption attackTargetOption;
        public int targetCount;
        public SkillEffect effectPrefab;
      
    }
}