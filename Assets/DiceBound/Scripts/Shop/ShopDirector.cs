using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound.Shop
{
    public class ShopDirector : DirectorBase
    {
        private List<BoosterPackDataTableRow> _boosterPackDataList;

        [SerializeField] private ImageWidget purchaseArea;
        [SerializeField] private List<PurchaseWidget> purchaseWidgets;

        private WalletDirector _walletDirector;
        private Dictionary<string, List<BoosterPackItemPoolDataTableRow>> _boosterPackItemDataList;
        private UnitDirector _unitDirector;
        private BoosterPackWidget _draggingBoosterPack;
        private Vector3 _dragOffset;


        public override IEnumerator OnInitialize()
        {
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _boosterPackDataList = DataTableManager.FindAllRows<BoosterPackDataTableRow>();
            _boosterPackItemDataList = DataTableManager.FindAllRows<BoosterPackItemPoolDataTableRow>()
                .GroupBy(x => x.boosterPackId).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var widget in purchaseWidgets)
            {
                widget.onDragBeginAction += OnBoosterPackDragBegin;
                widget.onDragBeginAction += OnBoosterPackDragEnd;
            }
            yield return null;
        }

        private void OnBoosterPackDragEnd(PurchaseWidget widget)
        {
            var isOverlapping = WidgetUtility.IsOverlapping(_draggingBoosterPack.rectTransform, purchaseArea.rectTransform);
            if (isOverlapping)
            {
                TryPurchase(widget);
            }
            else
            {
                _draggingBoosterPack.Rewind();
            }

            purchaseArea.Hide();   
            _draggingBoosterPack = null;
        }

        private void OnBoosterPackDragBegin(PurchaseWidget widget)
        {
            purchaseArea.Show();   
            _draggingBoosterPack = widget.GetBoosterPack();
            _dragOffset = widget.transform.position - InputManager.GetScreenPointerPosition();
        }

        public void Update()
        {
            if (_draggingBoosterPack)
            {
                _draggingBoosterPack.transform.position = InputManager.GetScreenPointerPosition() + _dragOffset;
            }
        }

        public void Refresh()
        {
            var dataList = PickRandomBoosterPackData(3);
            for (int i = 0; i < dataList.Count; i++)
            {
                purchaseWidgets[i].Setup(dataList[i]);
            }
        }

        public void TryPurchase(PurchaseWidget widget)
        {
            var gold = widget.GetCost();
            if (_walletDirector.HasGold(gold))
            {
                _walletDirector.SpendGold(gold);
                StartCoroutine(OpenBoosterPackRoutine(widget));
            }
        }

        private IEnumerator OpenBoosterPackRoutine(PurchaseWidget widget)
        {
            var data = widget.GetData();
            var pack = widget.GetBoosterPack();
            yield return pack.PlayOpenTween();
            widget.Hide();
            pack.Rewind();
            //일단 무조건 한번에 아이템 수량 1개
            var item = PickRandomBoosterPackItem(data);
            GainBoosterPackItem(item);
            yield return null;
        }

        private void GainBoosterPackItem(BoosterPackItemPoolDataTableRow item)
        {
            switch (item.itemType)
            {
                case BoosterPackItemType.Unit:
                    _unitDirector.SpawnUnit(item.itemId,UnitGroup.Ally);
                    break;
                
            }
        }

        private BoosterPackItemPoolDataTableRow PickRandomBoosterPackItem(BoosterPackDataTableRow data)
        {
            return _boosterPackItemDataList[data.id].GetRandomElementWithWeight(x => x.weight);
        }

        public List<BoosterPackDataTableRow> PickRandomBoosterPackData(int number)
        {
            return _boosterPackDataList.GetRandomElements(number);
        }
    }
}