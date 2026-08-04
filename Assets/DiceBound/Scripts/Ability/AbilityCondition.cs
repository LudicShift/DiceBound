using KCoreKit;

namespace DiceBound
{
    public class AbilityCondition
    {
        public static bool Always(AbilityEffect effect, AbilityConditionDataTableRow data, ref AbilityContext context)
        {
            return true;
        }
    }
}