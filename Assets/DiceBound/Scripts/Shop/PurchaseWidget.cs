using DiceBound;
using DiceBound.Interface;
using KCoreKit;
using UnityEngine;

public class PurchaseWidget : WidgetBase
{
    [SerializeField] private TextWidget combinationText;
    [SerializeField] private TextWidget amountText;
    [SerializeField] private ImageWidget itemImage;
    [SerializeField] private ImageWidget soldOutImage;
    [SerializeField] public ButtonWidget purchaseButton;

    private int _amount;
    private CombinationBase _combination;
    private IPurchaseItem _item;
    public ItemType itemType;
    private bool _soldOut;

    public void SetCombination(CombinationBase combination)
    {
        _combination = combination;
        combinationText.SetText(_combination.GetName());
    }

    public void SetItem(ItemType type, IPurchaseItem item)
    {
        itemType = type;
        _item = item;
        itemImage.SetSprite(_item.GetSprite());
    }

    public void SetAmount(int amount)
    {
        _amount = amount;
        amountText.SetText($"x{_amount}");
    }

    public CombinationBase GetCombination()
    {
        return _combination;
    }

    public IPurchaseItem GetItem()
    {
        return _item;
    }

    public int GetAmount()
    {
        return _amount;
    }

    public bool IsSoldOut()
    {
        return _soldOut;
    }

    public void SetSoldOut(bool value)
    {
        _soldOut = value;
        if (_soldOut)
        {
            soldOutImage.Show();
        }
        else
        {
            soldOutImage.Hide();
        }
    }
}