using KCoreKit;

namespace DiceBound
{
    public enum  ItemType
    {
        Unit
    
    }
    
    public class PurchaseItemDataTableRow : DataTableRowBase
    {
        public ItemType itemType;
        public string itemId;
        public string combinationID;
        public int amount;
    }
}