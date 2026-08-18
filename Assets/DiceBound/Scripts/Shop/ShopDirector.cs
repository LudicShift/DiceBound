using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceBound.Interface;
using KCoreKit;
using Unity.VisualScripting;
using UnityEngine;

namespace DiceBound
{
    public class ShopDirector : DirectorBase
    {
        [BigHeader("General")] [SerializeField]
        private Canvas canvas;

        [BigHeader("Widget")] [SerializeField] private ButtonWidget shopButtonWidget;
        [SerializeField] private ButtonWidget backToFieldButton;
        [SerializeField] private ButtonWidget rollDiceButtonWidget;
        [SerializeField] private TextWidget goldText;

        private int rollDiceCost = 100;
        private WalletDirector _walletDirector;
        private DiceDirector _diceDirector;
        private bool _isEnable = true;
        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;

        private List<PurchaseItemDataTableRow> _purchaseItemDataList;
        private List<PurchaseWidget> _purchaseWidgets;

        public override IEnumerator OnInitialize()
        {
            _purchaseItemDataList = DataTableManager.FindAllRows<PurchaseItemDataTableRow>();
            _purchaseWidgets = canvas.GetComponentsInChildren<PurchaseWidget>(true).ToList();
            foreach (var widget in _purchaseWidgets)
            {
                widget.purchaseButton.onClickAction += () => OnPurchaseButtonClick(widget);
            }

            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _diceDirector = DirectorFacade.GetDirector<DiceDirector>();
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            shopButtonWidget.onClickAction += ShowCanvas;
            backToFieldButton.onClickAction += HideCanvas;
            rollDiceButtonWidget.onClickAction += OnRollDiceButtonClick;
            yield return null;
        }

        private void OnPurchaseButtonClick(PurchaseWidget widget)
        {
            var result = _diceDirector.GetResult();
            if (result != null && result.Contains(widget.GetCombination()) && !widget.IsSoldOut())
            {
                _diceDirector.ClearDices();
                rollDiceButtonWidget.Show();
                for (int i = 0; i < widget.GetAmount(); i++)
                {
                    PurchaseItem(widget.itemType, widget.GetItem());
                    widget.SetSoldOut(true);
                }
            }
        }

        private void PurchaseItem(ItemType type, IPurchaseItem item)
        {
            switch (type)
            {
                case ItemType.Unit:
                    _unitDirector.SpawnUnit(item.GetId(),UnitGroup.Ally);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void OnRoundBegin()
        {
            //아이템 풀 변경 할수도 있음
            for (int i = 0; i < _purchaseItemDataList.Count; i++)
            {
                var data = _purchaseItemDataList[i];
                var combination = _diceDirector.GetCombination(data.combinationID);
                IPurchaseItem item;
                switch (data.itemType)
                {
                    case ItemType.Unit:
                        item = _unitDirector.GetUnitData(data.itemId);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _purchaseWidgets[i].SetCombination(combination);
                _purchaseWidgets[i].SetItem(data.itemType, item);
                _purchaseWidgets[i].SetAmount(data.amount);
                _purchaseWidgets[i].Show();
                _purchaseWidgets[i].SetSoldOut(false);
            }
        }

        public void OnRollDiceButtonClick()
        {
            if (_walletDirector.HasGold(rollDiceCost))
            {
                _walletDirector.SpendGold(rollDiceCost);
                goldText.SetText($"{_walletDirector.GetGold()}G");
                rollDiceButtonWidget.Hide();
                _diceDirector.Setup();
            }
        }

        private void HideCanvas()
        {
            canvas.gameObject.SetActive(false);
            _unitPlaceDirector.SetEnable(true);
        }

        private void ShowCanvas()
        {
            if (_isEnable)
            {
                _unitPlaceDirector.SetEnable(false);
                goldText.SetText($"{_walletDirector.GetGold()}G");
                canvas.gameObject.SetActive(true);
            }
        }

        public void SetEnable(bool value)
        {
            _isEnable = value;
        }
    }
}