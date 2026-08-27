using KCoreKit;
using UnityEngine;

namespace DiceBound.Shop
{
    public enum BoosterPackItemType
    {
        Unit
    }
    public class BoosterPackItemPoolDataTableRow : DataTableRowBase
    {
        public string boosterPackId;
        public BoosterPackItemType itemType;
        public string itemId;
        public float weight;
    }
}