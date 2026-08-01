using KCoreKit;

namespace YatchDungeon
{
    public class AbilityCondition
    {
        public static bool Always(AbilityEffect effect, AbilityPropertySet propertySet, ref AbilityContext context)
        {
            return true;
        }
    }
}