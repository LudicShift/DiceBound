using KCoreKit;
using UnityEditor.Animations;

namespace DiceBound
{
    public enum UnitAttackType
    {
        Melee,
        Ranged,
        Magic
    }

    public enum UnitGroup
    {
        Ally,
        Enemy
    }
    
    public class UnitDataTableRow : DataTableRowBase
    {
        public UnitGroup group;
        public UnitAttackType attackType;
        public string nameKey;
        public string descKey;
        public string roleKey;
        
        public string skillBasicKey;
        public string skillPassiveKey;
        public string skillActiveKey;
      
        public AnimatorController animator;

        public string grade;
        public int maxSkillSlot;

        public int hp;
        public int str;
        public int con;
        public int dex;
        public int mag;
        public int def;
        public int mdf;
        public int spd;
        public float height;
    }
}