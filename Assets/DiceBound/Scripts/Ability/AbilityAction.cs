using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class AbilityAction
    {
        private static BattleDirector _battleDirector;
        private static UnitDirector _unitDirector;

        public static void Setup()
        {
            _battleDirector = DirectorFacade.GetDirector<BattleDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
        }

        public static void MeleeAttack(AbilityEffect effect, AbilityActionDataTableRow data, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    var targetStatAgent = target.GetStatAgent();
                    BasicAttack(abilityContext, target,
                        StatUtility.GetApMelee(statAgent) * (1 - StatUtility.GetMitigationP(targetStatAgent)));
                }
            });
        }

        private static void BasicAttack(AbilityContext context, UnitCore target, float damage)
        {
            _battleDirector.EnqueueContext(new BattleContext()
            {
                self = context.self,
                target = target,
                castTime = context.castTime,
                damage = damage,
                skillEffectKey = context.skillEffectKey
            });
        }

        private static void Healing(AbilityContext context, UnitCore target, float healPower)
        {
            _battleDirector.EnqueueContext(new BattleContext()
            {
                self = context.self,
                target = target,
                healPower = healPower,
                skillEffectKey = context.skillEffectKey
            });
        }

        public static void RangedAttack(AbilityEffect effect, AbilityActionDataTableRow data,
            ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    var targetStatAgent = target.GetStatAgent();
                    BasicAttack(abilityContext, target,
                        StatUtility.GetApRanged(statAgent) * (1 - StatUtility.GetMitigationP(targetStatAgent)));
                }
            });
        }

        public static void MagicAttack(AbilityEffect effect, AbilityActionDataTableRow data, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    var targetStatAgent = target.GetStatAgent();
                    BasicAttack(abilityContext, target,
                        StatUtility.GetApMagic(statAgent) * (1 - StatUtility.GetMitigationM(targetStatAgent)));
                }
            });
        }

        public static void Heal(AbilityEffect effect, AbilityActionDataTableRow data, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.AttackTargetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            context.self.ShowAttackAnimation(() =>
            {
                foreach (var target in targetList)
                {
                    Healing(abilityContext, target, StatUtility.GetHealPower(statAgent));
                }
            });
        }
    }
}