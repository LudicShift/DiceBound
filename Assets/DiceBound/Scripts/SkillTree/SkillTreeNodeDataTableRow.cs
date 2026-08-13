using System.Collections.Generic;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class SkillTreeNodeDataTableRow : DataTableRowBase
    {
        public string nameKey;
        public string descKey;
        public Sprite icon;
        public int diamondCost;
        public string effectKey;
        public float effectValue;
        public List<string> prerequisiteIds;
    }
}
