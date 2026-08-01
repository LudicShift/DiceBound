using KCoreKit;

namespace YatchDungeon
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
        
        public UnitCore prefab;
        
        public int hp;
        public int str;
        public int con;
        public int dex;
        public int mag;
        public int def;
        public int mdf;
        public int spd;
    }
}