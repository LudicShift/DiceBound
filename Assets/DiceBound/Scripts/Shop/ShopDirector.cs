using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceBound.Shop
{
    public class ShopDirector : DirectorBase
    {
        private List<BoosterPackDataTableRow> _boosterPackDataList;

        [SerializeField] private Canvas shopCanvas;
        [SerializeField] private ImageWidget purchaseArea;
        [SerializeField] private List<PurchaseWidget> purchaseWidgets;

        private WalletDirector _walletDirector;
        private Dictionary<string, List<BoosterPackItemPoolDataTableRow>> _boosterPackItemDataList;
        private UnitDirector _unitDirector;
        private PurchaseWidget _draggingPurchseWidget;
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
          
            }
            InputManager.RegisterAction("Click",PlayerActionType.Canceled,OnBoosterPackDragEnd);
            yield return null;
        }

        private void OnBoosterPackDragEnd(InputAction.CallbackContext obj)
        {
            if (!_draggingPurchseWidget)
            {
                return;
            }

            var boosterPack = _draggingPurchseWidget.GetBoosterPack();
            var isOverlapping = WidgetUtility.IsOverlapping(boosterPack.rectTransform, purchaseArea.rectTransform);
            if (isOverlapping)
            {
                TryPurchase(_draggingPurchseWidget);
            }
            else
            {
                boosterPack.Rewind();
            }

            purchaseArea.Hide();   
            
            _draggingPurchseWidget = null;
        }

        private void OnBoosterPackDragBegin(PurchaseWidget widget)
        {
            purchaseArea.Show();   
            _draggingPurchseWidget = widget;
            _dragOffset = widget.transform.position - InputManager.GetScreenPointerPosition(shopCanvas);
        }

        public void Update()
        {
            if (_draggingPurchseWidget)
            {
                var boosterPack = _draggingPurchseWidget.GetBoosterPack();
                boosterPack.transform.position = InputManager.GetScreenPointerPosition(shopCanvas) + _dragOffset;
            }
        }

        public void Refresh()
        {
            for (int i = 0; i < _boosterPackDataList.Count; i++)
            {
                purchaseWidgets[i].Setup(_boosterPackDataList[i]);
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
            else
            {
                widget.GetBoosterPack().Rewind();
            }
        }

        private IEnumerator OpenBoosterPackRoutine(PurchaseWidget widget)
        {
            var data = widget.GetData();
            var pack = widget.GetBoosterPack();
            yield return pack.PlayOpenTween();
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