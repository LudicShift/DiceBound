using System;
using KCoreKit;
using UnityEngine;

namespace DiceBound.Shop
{
    public class PurchaseWidget : WidgetBase
    {
        [SerializeField]
        private ImageWidget preview;
        [SerializeField]
        private BoosterPackWidget boosterPack;
        [SerializeField]
        private TextWidget costText;
        private int _cost;
        private BoosterPackDataTableRow _data;
        public Action<PurchaseWidget> onDragBeginAction;

        public void Awake()
        {
            boosterPack.onPointerDownAction += _ => onDragBeginAction.Invoke(this);
        }

        public void Setup(BoosterPackDataTableRow data)
        {
            _data = data;
            _cost = data.cost;
            boosterPack.Setup(data);
            costText.SetText(_cost.ToString());
            preview.SetSprite(data.texture.ToSprite());
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
            return boosterPack;
        }
    }
}