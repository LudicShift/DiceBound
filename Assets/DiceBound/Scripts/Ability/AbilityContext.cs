using KCoreKit;

namespace DiceBound
{
    public class AbilityContext : IAbilityContext
    {
        public UnitCore self;
        public string skillEffectKey;
        public SkillTargetOption targetOption;
        public float castTime;
        public int targetCount;
        public string animClip;
    }
}