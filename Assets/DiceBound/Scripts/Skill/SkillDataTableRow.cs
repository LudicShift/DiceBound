using KCoreKit;

namespace DiceBound
{
    public enum SkillType
    {
        Basic,
        Active,
        Passive
    }
    
    public enum SkillTargetGroup
    {
        Ally,
        Enemy,
        Self
    }
            
    public enum SkillTargetOption
    {
        General,
        Weak,
        Strong,
        LessHp,
        Random
    }
        
    public class SkillDataTableRow : DataTableRowBase
    {
        public SkillType skillType;
        public string nameKey;
        public string descKey;
        public string abilityId;
        public float cooldown;
        public float castTime;
        public string effectKey;
        public int targetCount;
        public string animClip;
        public int priority;
        public float startUpDelay;
        
        public SkillTargetGroup targetGroup;
        public SkillTargetOption targetOption;
       
    }
}