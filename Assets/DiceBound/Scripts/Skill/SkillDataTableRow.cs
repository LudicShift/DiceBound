using KCoreKit;

namespace DiceBound
{
    public enum SkillType
    {
        Basic,
        Active,
        Passive
    }
    public enum SkillTargetOption
    {
        Ally,
        Enemy,
        Self
    }
        
    public class SkillDataTableRow : DataTableRowBase
    {
        public SkillType skillType;
        public string nameKey;
        public string descKey;
        public string abilityId;
        public float cooldown;
        public float castTime;
        public SkillTargetOption targetOption;
        public string effectKey;
        public int targetCount;
        public string animClip;
    }
}