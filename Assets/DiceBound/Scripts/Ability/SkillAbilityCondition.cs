using KCoreKit;

namespace DiceBound
{
    public class SkillAbilityCondition
    {
        public static bool Always(AbilityEffect effect, AbilityConditionDataTableRow data, ref SkillAbilityContext context)
        {
            return true;
        }
    }
}