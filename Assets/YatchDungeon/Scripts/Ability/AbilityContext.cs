using KCoreKit;

namespace YatchDungeon
{
    public class AbilityContext : IAbilityContext
    {
        public UnitCore self;
        public SkillEffect skillEffectPrefab;
        public UnitAttackTargetOption AttackTargetOption;
        public float castTime;
        public int targetCount;
    }
}