using System.Collections.Generic;

namespace DiceBound
{
    public class BattleContext
    {
        public UnitCore self;
        public UnitCore target;

        public float priority;
        public float damage;
        public string skillEffectKey;
        public float healPower;
        public float startUpDelay;
        public string animClip { get; set; }
    }
}