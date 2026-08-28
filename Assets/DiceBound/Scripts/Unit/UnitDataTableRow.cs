using DiceBound.Interface;
using KCoreKit;
using UnityEngine;

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
    
    public class UnitDataTableRow : DataTableRowBase, IPurchaseItem
    {
        public UnitAttackType attackType;
        public string nameKey;
        public string descKey;
        public string roleKey;
        
        public string skillBasicKey;
        public string skillPassiveKey;
        public string skillActiveKey;
      
        public Texture2D texture;
        public RuntimeAnimatorController animator;

        public string grade;
        public string race;
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
        public Sprite GetSprite()
        {
            return texture.ToSprite();
        }

        public string GetId()
        {
            return id;
        }
    }
}