using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    /// <summary>
    /// 웨이브 밸런스 테스트용 순수 계산 유틸리티.
    /// StatAgent/GameObject 없이 UnitDataTableRow 원본 스탯만으로 StatUtility와 동일한 공식을 재현한다.
    /// </summary>
    public struct BalanceUnit
    {
        public UnitDataTableRow data;

        public BalanceUnit(UnitDataTableRow data)
        {
            this.data = data;
        }
    }

    public static class WaveBalanceCalculator
    {
        // UnitCore.Upgrade()가 성급마다 곱하는 StatModifier(0.8f, PercentMult)와 동일한 누적 배율.
        public static float GetTierMultiplier(int tier)
        {
            return Mathf.Pow(1.8f, tier);
        }

        public static float Str(BalanceUnit u) => u.data.str;
        public static float Dex(BalanceUnit u) => u.data.dex;
        public static float Mag(BalanceUnit u) => u.data.mag;
        public static float Def(BalanceUnit u) => u.data.def;
        public static float Mdf(BalanceUnit u) => u.data.mdf;
        public static float Spd(BalanceUnit u) => u.data.spd;
        public static float Con(BalanceUnit u) => u.data.con;
        public static float Hp(BalanceUnit u) => u.data.hp;

        public static float GetAttackInterval(BalanceUnit u)
        {
            return Mathf.Clamp(2.0f * (100f / Spd(u)), 0.4f, 99f);
        }

        public static float GetMaxHp(BalanceUnit u)
        {
            return Hp(u) + Con(u) * 5f;
        }

        public static float GetApMelee(BalanceUnit u) => Str(u) + Dex(u) * 0.5f;
        public static float GetApRanged(BalanceUnit u) => Dex(u) + Str(u) * 0.3f;
        public static float GetApMagic(BalanceUnit u) => Mag(u) * 1.2f;

        public static float GetCritRate(BalanceUnit u)
        {
            return Mathf.Min(0.05f + Dex(u) * 0.004f, 0.50f);
        }

        public const float CritMult = 1.5f;

        /// <summary>BattleDirector.ExecuteBattleContext와 동일한 인자 순서: GetDodgeRate(target, self).</summary>
        public static float GetDodgeRate(BalanceUnit target, BalanceUnit self)
        {
            return Mathf.Clamp(Dex(self) * 0.002f - Dex(target) * 0.0015f, 0f, 0.25f);
        }

        public static float GetMitigationP(BalanceUnit u)
        {
            return Mathf.Min(Def(u) / (Def(u) + 100f), 0.75f);
        }

        public static float GetMitigationM(BalanceUnit u)
        {
            return Mathf.Min(Mdf(u) / (Mdf(u) + 100f), 0.75f);
        }

        /// <summary>abilityId(MeleeAttack/RangedAttack/MagicAttack)로부터 실제 데미지 타입을 구한다.
        /// 매칭되지 않으면(예: Heal) fallback(보통 공격자 본연의 attackType)을 그대로 쓴다.</summary>
        private static UnitAttackType GetAbilityAttackType(string abilityId, UnitAttackType fallback)
        {
            switch (abilityId)
            {
                case "MeleeAttack": return UnitAttackType.Melee;
                case "RangedAttack": return UnitAttackType.Ranged;
                case "MagicAttack": return UnitAttackType.Magic;
                default: return fallback;
            }
        }

        private static float GetApByType(BalanceUnit u, UnitAttackType attackType)
        {
            switch (attackType)
            {
                case UnitAttackType.Melee: return GetApMelee(u);
                case UnitAttackType.Ranged: return GetApRanged(u);
                case UnitAttackType.Magic: return GetApMagic(u);
            }

            return GetApMelee(u);
        }

        /// <summary>물리(Melee/Ranged)면 DEF, 마법이면 MDF — 공격 타입은 공격자(스킬)가 정하고, 경감은 방어자 스탯으로 계산한다.</summary>
        private static float GetMitigationByType(BalanceUnit defender, UnitAttackType attackType)
        {
            return attackType == UnitAttackType.Magic ? GetMitigationM(defender) : GetMitigationP(defender);
        }

        /// <summary>평균적인 상대 팀 한 명을 상대로 한 방(타겟 1명 기준) 기대 데미지.</summary>
        private static float GetExpectedHitDamage(BalanceUnit attacker, UnitAttackType attackType, List<BalanceUnit> opposingTeam)
        {
            if (opposingTeam == null || opposingTeam.Count == 0) return 0f;

            float ap = GetApByType(attacker, attackType);
            float critRate = GetCritRate(attacker);
            float sum = 0f;

            foreach (var defender in opposingTeam)
            {
                float mitigation = GetMitigationByType(defender, attackType);
                float baseHit = ap * (1f - mitigation);
                float expectedHit = baseHit * (1f - critRate) + baseHit * CritMult * critRate;
                float dodgeRate = GetDodgeRate(defender, attacker);
                expectedHit *= (1f - dodgeRate);
                sum += expectedHit;
            }

            return sum / opposingTeam.Count;
        }

        /// <summary>기본 공격(skillBasicKey) 한 유닛의 기대 DPS. Skill.OnUpdate의 Basic 타입과 동일하게 GetAttackInterval마다 발동.
        /// skillBasicKey 자체의 targetOption/abilityId를 우선 참조한다 — Priest처럼 기본 공격도 힐(Ally 대상)인 유닛은 0을 반환한다.</summary>
        public static float GetBasicDps(BalanceUnit attacker, List<BalanceUnit> opposingTeam,
            Dictionary<string, SkillDataTableRow> skillDictionary)
        {
            var attackType = attacker.data.attackType;

            if (skillDictionary != null && !string.IsNullOrEmpty(attacker.data.skillBasicKey) &&
                skillDictionary.TryGetValue(attacker.data.skillBasicKey, out var basicSkill))
            {
                if (basicSkill.targetGroup != SkillTargetGroup.Enemy) return 0f;
                attackType = GetAbilityAttackType(basicSkill.abilityId, attackType);
            }

            return GetExpectedHitDamage(attacker, attackType, opposingTeam) / GetAttackInterval(attacker);
        }

        /// <summary>액티브 스킬(skillActiveKey) 한 유닛의 기대 DPS. cooldown마다 발동하고 targetCount명을 동시에 타격.
        /// 데미지 타입은 스킬 자신의 abilityId로 판정하고(유닛 본연 타입과 다를 수 있음), targetOption이 Enemy가 아니면(힐/버프 등 아군·자신 대상) 0을 반환한다.</summary>
        public static float GetActiveSkillDps(BalanceUnit attacker, SkillDataTableRow skill, List<BalanceUnit> opposingTeam)
        {
            if (skill == null || !skill.isEnable || skill.skillType != SkillType.Active) return 0f;
            if (skill.targetGroup != SkillTargetGroup.Enemy) return 0f;
            if (skill.cooldown <= 0f) return 0f;
            if (opposingTeam == null || opposingTeam.Count == 0) return 0f;

            var abilityType = GetAbilityAttackType(skill.abilityId, attacker.data.attackType);
            int hitTargets = Mathf.Max(1, Mathf.Min(skill.targetCount, opposingTeam.Count));
            float hitDamage = GetExpectedHitDamage(attacker, abilityType, opposingTeam);
            return hitDamage * hitTargets / skill.cooldown;
        }

        /// <summary>한 유닛의 기본 공격 + 액티브 스킬(적 대상인 것만)을 합산한 기대 DPS.</summary>
        public static float GetExpectedUnitDps(BalanceUnit attacker, List<BalanceUnit> opposingTeam,
            Dictionary<string, SkillDataTableRow> skillDictionary)
        {
            float dps = GetBasicDps(attacker, opposingTeam, skillDictionary);

            if (skillDictionary != null && !string.IsNullOrEmpty(attacker.data.skillActiveKey) &&
                skillDictionary.TryGetValue(attacker.data.skillActiveKey, out var activeSkill))
            {
                dps += GetActiveSkillDps(attacker, activeSkill, opposingTeam);
            }

            return dps;
        }

        public static float ComputeTeamDps(List<BalanceUnit> team, List<BalanceUnit> opposingTeam,
            Dictionary<string, SkillDataTableRow> skillDictionary)
        {
            float total = 0f;
            foreach (var u in team)
            {
                total += GetExpectedUnitDps(u, opposingTeam, skillDictionary);
            }

            return total;
        }

        /// <summary>WaveDirector.PickEnemy()와 동일한 가중치 룰렛으로 웨이브 하나의 적 구성을 뽑는다.</summary>
        public static List<BalanceUnit> DrawWaveComposition(WaveDataTableRow wave,
            List<WaveEnemyPoolDataTableRow> pool, Dictionary<string, UnitDataTableRow> unitDictionary)
        {
            var result = new List<BalanceUnit>();
            //로직 수정에 의해 지워짐(To.claude)
            return result;
        }

        private class SimUnit
        {
            public BalanceUnit unit;
            public float hp;
            public float nextAttackTime;
            public UnitAttackType basicAttackType;
            public float nextSkillTime;
            public int skillTargetCount;
            public UnitAttackType skillAttackType;
        }

        private static void ApplyHit(SimUnit actor, UnitAttackType attackType, SimUnit target)
        {
            float dodgeRate = GetDodgeRate(target.unit, actor.unit);
            if (Random.value >= dodgeRate)
            {
                float ap = GetApByType(actor.unit, attackType);
                float mitigation = GetMitigationByType(target.unit, attackType);
                float damage = ap * (1f - mitigation);
                if (Random.value < GetCritRate(actor.unit)) damage *= CritMult;
                target.hp -= damage;
            }
        }

        /// <summary>몬테카를로 시뮬레이션으로 아군 승률을 추정한다. 매 시행마다 적 구성을 다시 뽑는다.
        /// 기본 공격과 액티브 스킬(적 대상인 것만)을 각자의 주기·데미지 타입대로 함께 시뮬레이션한다.</summary>
        public static float SimulateWinRate(List<BalanceUnit> allyTeam, WaveDataTableRow wave,
            List<WaveEnemyPoolDataTableRow> pool, Dictionary<string, UnitDataTableRow> unitDictionary,
            Dictionary<string, SkillDataTableRow> skillDictionary, int trials = 300)
        {
            if (allyTeam == null || allyTeam.Count == 0) return 0f;

            int wins = 0;
            const float timeCap = 120f;

            System.Func<BalanceUnit, SimUnit> makeSimUnit = u =>
            {
                var su = new SimUnit
                {
                    unit = u, hp = GetMaxHp(u), nextAttackTime = GetAttackInterval(u), basicAttackType = u.data.attackType,
                    nextSkillTime = float.MaxValue, skillTargetCount = 1, skillAttackType = u.data.attackType
                };

                if (skillDictionary != null && !string.IsNullOrEmpty(u.data.skillBasicKey) &&
                    skillDictionary.TryGetValue(u.data.skillBasicKey, out var basicSkill))
                {
                    if (basicSkill.targetGroup != SkillTargetGroup.Enemy) su.nextAttackTime = float.MaxValue;
                    else su.basicAttackType = GetAbilityAttackType(basicSkill.abilityId, u.data.attackType);
                }

                if (skillDictionary != null && !string.IsNullOrEmpty(u.data.skillActiveKey) &&
                    skillDictionary.TryGetValue(u.data.skillActiveKey, out var skill) &&
                    skill.isEnable && skill.skillType == SkillType.Active &&
                    skill.targetGroup == SkillTargetGroup.Enemy && skill.cooldown > 0f)
                {
                    su.nextSkillTime = skill.cooldown;
                    su.skillTargetCount = Mathf.Max(1, skill.targetCount);
                    su.skillAttackType = GetAbilityAttackType(skill.abilityId, u.data.attackType);
                }
                return su;
            };

            for (int t = 0; t < trials; t++)
            {
                var enemyTeam = DrawWaveComposition(wave, pool, unitDictionary);
                if (enemyTeam.Count == 0) continue;

                var allies = allyTeam.Select(makeSimUnit).ToList();
                var enemies = enemyTeam.Select(makeSimUnit).ToList();

                float time = 0f;
                bool allyWon = false;

                while (time < timeCap)
                {
                    var aliveAllies = allies.Where(x => x.hp > 0f).ToList();
                    var aliveEnemies = enemies.Where(x => x.hp > 0f).ToList();

                    if (aliveEnemies.Count == 0) { allyWon = true; break; }
                    if (aliveAllies.Count == 0) { allyWon = false; break; }

                    SimUnit actor = null;
                    bool actorIsAlly = false;
                    bool isSkill = false;
                    float earliest = float.MaxValue;
                    foreach (var a in aliveAllies)
                    {
                        if (a.nextAttackTime < earliest) { earliest = a.nextAttackTime; actor = a; actorIsAlly = true; isSkill = false; }
                        if (a.nextSkillTime < earliest) { earliest = a.nextSkillTime; actor = a; actorIsAlly = true; isSkill = true; }
                    }
                    foreach (var e in aliveEnemies)
                    {
                        if (e.nextAttackTime < earliest) { earliest = e.nextAttackTime; actor = e; actorIsAlly = false; isSkill = false; }
                        if (e.nextSkillTime < earliest) { earliest = e.nextSkillTime; actor = e; actorIsAlly = false; isSkill = true; }
                    }

                    time = earliest;
                    var targetPool = actorIsAlly ? aliveEnemies : aliveAllies;

                    if (isSkill)
                    {
                        int hitCount = Mathf.Min(actor.skillTargetCount, targetPool.Count);
                        var shuffled = targetPool.OrderBy(x => Random.value).ToList();
                        for (int i = 0; i < hitCount; i++) ApplyHit(actor, actor.skillAttackType, shuffled[i]);
                        actor.nextSkillTime += skillDictionary[actor.unit.data.skillActiveKey].cooldown;
                    }
                    else
                    {
                        var target = targetPool[Random.Range(0, targetPool.Count)];
                        ApplyHit(actor, actor.basicAttackType, target);
                        actor.nextAttackTime += GetAttackInterval(actor.unit);
                    }
                }

                if (allyWon) wins++;
            }

            return trials > 0 ? (float)wins / trials * 100f : 0f;
        }
    }
}
