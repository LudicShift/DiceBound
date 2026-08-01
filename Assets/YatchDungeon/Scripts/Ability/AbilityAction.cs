using KCoreKit;
using UnityEngine;

namespace YatchDungeon
{
    public class AbilityAction
    {
        private static UnitDirector _unitDirector;

        public static void Setup()
        {
            _unitDirector = DirectorFacade.GetSubMode<UnitDirector>();
        }
        
        public static void MeleeAttack(AbilityEffect effect, AbilityPropertySet propertySet, ref AbilityContext context)
        {
            
            var targetList = _unitDirector.GetTarget(context.self,context.AttackTargetOption,context.targetCount);
            foreach (var target in targetList)
            {
                context.self.Animate("Attack01");
                var instance = Object.Instantiate(context.skillEffectPrefab);
                instance.SetDamage(UnitUtility.GetApMelee(context.self));
                instance.Execute(target);
            }
        }
    }
}