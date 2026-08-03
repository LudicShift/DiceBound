using UnityEngine;

namespace YatchDungeon
{
    public static class UnitUtility
    {
        // 1. 공격 주기 (기존 구현)
        public static float GetAttackInterval(UnitCore unitCore)
        {
            return Mathf.Clamp(2.0f * (100 / unitCore.spd), 0.4f, 99f);
        }

        // 2. 최대 체력 (MAX_HP = HP + CON * 5)
        // 주의: 재료/유물 보정은 이 계산 전에 합산되어야 함
        public static float GetMaxHp(UnitCore unitCore)
        {
            return unitCore.hp + unitCore.con * 5f;
        }

        // 3. 근접 물리 공격력 (AP_MELEE = STR + DEX * 0.5)
        public static float GetApMelee(UnitCore unitCore)
        {
            return unitCore.str + unitCore.dex * 0.5f;
        }

        // 4. 원거리 물리 공격력 (AP_RANGED = DEX + STR * 0.3)
        public static float GetApRanged(UnitCore unitCore)
        {
            return unitCore.dex + unitCore.str * 0.3f;
        }

        // 5. 마법 공격력 (AP_MAGIC = MAG * 1.2)
        public static float GetApMagic(UnitCore unitCore)
        {
            return unitCore.mag * 1.2f;
        }

        // 6. 회복력 (HEAL_POWER = MAG)
        public static float GetHealPower(UnitCore unitCore)
        {
            return unitCore.mag;
        }

        // 7. 치타 확률 (CRIT_RATE = min(0.05 + DEX * 0.004, 0.50))
        public static float GetCritRate(UnitCore unitCore)
        {
            return Mathf.Min(0.05f + unitCore.dex * 0.004f, 0.50f);
        }

        // 8. 치명타 배율 (CRIT_MULT = 1.5)
        public static float GetCritMult(UnitCore unitCore)
        {
            return 1.5f;
        }

        // 9. 회피 확률 (DODGE_RATE = clamp(DEX_def * 0.002 - DEX_atk * 0.0015, 0, 0.25))
        public static float GetDodgeRate(UnitCore defenderCore, UnitCore attackerCore)
        {
            return Mathf.Clamp(defenderCore.dex * 0.002f - attackerCore.dex * 0.0015f, 0f, 0.25f);
        }

        // 10. 물리 경감 (MITIGATION_P = min(DEF / (DEF + 100), 0.75))
        public static float GetMitigationP(UnitCore target)
        {
            return Mathf.Min(target.def / (target.def + 100f), 0.75f);
        }

        // 11. 마법 경감 (MITIGATION_M = min(MDF / (MDF + 100), 0.75))
        public static float GetMitigationM(UnitCore target)
        {
            return Mathf.Min(target.mdf / (target.mdf + 100f), 0.75f);
        }

        // 12. 상태이상 저항 (STATUS_RESIST = min(CON * 0.005, 0.40))
        public static float GetStatusResist(UnitCore unitCore)
        {
            return Mathf.Min(unitCore.con * 0.005f, 0.40f);
        }

        // 13. 관통 피해 배율 (PIERCE_FALLOFF = 1 - 0.2 * (N - 1))
        public static float GetPierceFalloff(int n)
        {
            return 1f - 0.2f * (n - 1);
        }
    }
}