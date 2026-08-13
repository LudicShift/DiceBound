using KCoreKit;

namespace DiceBound
{
    public class AbilityContext : IAbilityContext
    {
        public UnitCore self;
        public string skillEffectKey;
        public SkillTargetGroup targetGroup;
        public SkillTargetOption targetOption;
        public float castTime;
        public float priority;
        public int targetCount;
        public string animClip;
        public float startUpDelay;
    }
}