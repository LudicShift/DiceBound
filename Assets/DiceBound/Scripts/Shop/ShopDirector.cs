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
        [SerializeField] private RectTransform floatingParent;
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
                boosterPack.transform.SetParent(_draggingPurchseWidget.transform);
                boosterPack.Rewind();
            }

            purchaseArea.Hide();   
            
            _draggingPurchseWidget = null;
        }

        private void OnBoosterPackDragBegin(PurchaseWidget widget)
        {
            purchaseArea.Show();  
            _draggingPurchseWidget = widget;
            var boosterPack = _draggingPurchseWidget.GetBoosterPack();
            boosterPack.transform.SetParent(floatingParent,true);
            
            _dragOffset = boosterPack.transform.position - InputManager.GetScreenPointerPosition(shopCanvas);
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
            // 로스터가 꽉 찬 상태로 유닛을 얻으면 배치할 칸이 없어 원점에 방치되므로, 애초에 구매 자체를 막는다.
            if (_walletDirector.HasGold(gold) && !_unitDirector.IsAllyFull())
            {
                _walletDirector.SpendGold(gold);
                StartCoroutine(OpenBoosterPackRoutine(widget));
            }
            else
            {
                var boosterPack = widget.GetBoosterPack();
                boosterPack.transform.SetParent(widget.transform);
                boosterPack.Rewind();
            }
        }

        private IEnumerator OpenBoosterPackRoutine(PurchaseWidget widget)
        {
            var data = widget.GetData();
            var boosterPack = widget.GetBoosterPack();
            yield return boosterPack.PlayOpenTween();
            boosterPack.transform.SetParent(widget.transform);
            boosterPack.Rewind();
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