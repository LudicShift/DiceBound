using System;
using KCoreKit;

namespace DiceBound.Shop
{
    public class PurchaseWidget : WidgetBase
    {
        private BoosterPackWidget _boosterPack;
        private TextWidget _costText;
        private int _cost;
        private BoosterPackDataTableRow _data;
        public Action<PurchaseWidget> onDragBeginAction;
        public Action<PurchaseWidget> onDragEndAction;

        public void Awake()
        {
            _boosterPack = GetComponentInChildren<BoosterPackWidget>();
            _costText = GetComponentInChildren<TextWidget>();
        }

        public void Setup(BoosterPackDataTableRow data)
        {
            _data = data;
            _boosterPack.Setup(data);
            _cost = data.cost;
            _costText.SetText(_cost.ToString());
        }

        public int GetCost()
        {
            return _cost;
        }

        public BoosterPackDataTableRow GetData()
        {
            return _data;
        }

        public BoosterPackWidget GetBoosterPack()
        {
            return _boosterPack;
        }
    }
}