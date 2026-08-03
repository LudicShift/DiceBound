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
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    BasicAttack(abilityContext, target,
                        UnitUtility.GetApMelee(abilityContext.self) * (1 - UnitUtility.GetMitigationP(target)));
                }
            });
        }

        private static void BasicAttack(AbilityContext context, UnitCore target, float damage)
        {
            var instance = Object.Instantiate(context.skillEffectPrefab);
            instance.transform.position = context.self.transform.position;
            instance.SetDamage(damage);
            instance.SetDirection(context.self.group == UnitGroup.Ally);
            instance.Execute(target);
        }

        private static void Healing(AbilityContext context, UnitCore target, float healPower)
        {
            var instance = Object.Instantiate(context.skillEffectPrefab);
            instance.transform.position = context.self.transform.position;
            instance.SetHeal(true);
            instance.SetDamage(healPower);
            instance.SetDirection(context.self.group == UnitGroup.Ally);
            instance.Execute(target);
        }

        public static void RangedAttack(AbilityEffect effect, AbilityPropertySet propertySet,
            ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    BasicAttack(abilityContext, target,
                        UnitUtility.GetApRanged(abilityContext.self) * (1 - UnitUtility.GetMitigationP(target)));
                }
            });
        }

        public static void MagicAttack(AbilityEffect effect, AbilityPropertySet propertySet, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    BasicAttack(abilityContext, target,
                        UnitUtility.GetApMagic(abilityContext.self) *(1-UnitUtility.GetMitigationM(target)) );
                }
            });
        }

        public static void Heal(AbilityEffect effect, AbilityPropertySet propertySet, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    Healing(abilityContext, target, UnitUtility.GetHealPower(abilityContext.self));
                }
            });
        }
    }
}