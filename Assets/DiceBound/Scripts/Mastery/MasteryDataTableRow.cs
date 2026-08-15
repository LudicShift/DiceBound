using System.Collections.Generic;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MasteryDataTableRow : DataTableRowBase
    {
        public string nameKey;
        public string descKey;
        public int cost;
        public string effectKey;
        public float effectValue;
        public List<string> prerequisiteIds;
    }
}
