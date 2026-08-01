using KCoreKit;

namespace YatchDungeon
{
    public class AbilityContext : IAbilityContext
    {
        public UnitCore self;
        public SkillEffect skillEffectPrefab;
        public UnitTargetOption targetOption;
        public float castTime;
        public int targetCount;
    }
}