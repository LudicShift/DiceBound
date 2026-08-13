using System;
using System.Collections.Generic;
using KCoreKit;

namespace DiceBound
{
    [Serializable]
    public class SkillTreeSaveData : ISerializeData
    {
        public List<string> unlockedNodeIds = new List<string>();
        public int diamond;
    }
}
