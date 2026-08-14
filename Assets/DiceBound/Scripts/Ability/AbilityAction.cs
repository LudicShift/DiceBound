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
            var targetList = _unitDirector.GetTarget(context.self, context.targetGroup,context.targetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            foreach (var target in targetList)
            {
                var targetStatAgent = target.GetStatAgent();
                _battleDirector.EnqueueContext(new BattleContext()
                {
                    self = context.self,
                    priority = context.priority,
                    target = target,
                    animClip = context.animClip,
                    startUpDelay = context.startUpDelay,
                    damage = StatUtility.GetApMelee(statAgent) * (1 - StatUtility.GetMitigationP(targetStatAgent)),
                    skillEffectKey = context.skillEffectKey
                });
            }
        }



        private static void Healing(AbilityContext context, UnitCore target, float healPower)
        {
            _battleDirector.EnqueueContext(new BattleContext()
            {
                self = context.self,
                priority = context.priority,
                target = target,
                animClip = context.animClip,
                startUpDelay = context.startUpDelay,
                healPower = healPower,
                skillEffectKey = context.skillEffectKey
            });
        }

        public static void RangedAttack(AbilityEffect effect, AbilityActionDataTableRow data,
            ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.targetGroup, context.targetOption, context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            
            foreach (var target in targetList)
            {
                var targetStatAgent = target.GetStatAgent();
                _battleDirector.EnqueueContext(new BattleContext()
                {
                    self = context.self,
                    priority = context.priority,
                    target = target,
                    animClip = context.animClip,
                    startUpDelay = context.startUpDelay,
                    damage = StatUtility.GetApRanged(statAgent) * (1 - StatUtility.GetMitigationP(targetStatAgent)),
                    skillEffectKey = context.skillEffectKey
                });
            }
        }

        public static void MagicAttack(AbilityEffect effect, AbilityActionDataTableRow data, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.targetGroup, context.targetOption,context.targetCount);
            var abilityContext = context;
            var statAgent = abilityContext.self.GetStatAgent();
            
            foreach (var target in targetList)
            {
                var targetStatAgent = target.GetStatAgent();
                _battleDirector.EnqueueContext(new BattleContext()
                {
                    self = context.self,
                    priority = context.priority,
                    target = target,
                    animClip = context.animClip,
                    startUpDelay = context.startUpDelay,
                    damage =  StatUtility.GetApMagic(statAgent) * (1 - StatUtility.GetMitigationM(targetStatAgent)),
                    skillEffectKey = context.skillEffectKey
                });
            }
          
        }

        public static void Heal(AbilityEffect effect, AbilityActionDataTableRow data, ref AbilityContext context)
        {
            var targetList = _unitDirector.GetTarget(context.self, context.targetGroup, context.targetOption, context.targetCount);
            var statAgent = context.self.GetStatAgent();
            
            foreach (var target in targetList)
            {
                var targetStatAgent = target.GetStatAgent();
                _battleDirector.EnqueueContext(new BattleContext()
                {
                    self = context.self,
                    priority = context.priority,
                    target = target,
                    animClip = context.animClip,
                    startUpDelay = context.startUpDelay,
                    healPower =  StatUtility.GetHealPower(statAgent),
                    skillEffectKey = context.skillEffectKey
                });
            }
        }
    }
}
