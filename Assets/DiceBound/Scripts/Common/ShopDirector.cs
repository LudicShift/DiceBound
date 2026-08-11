using System.Collections;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class ShopDirector : DirectorBase
    {
        [SerializeField] private ButtonWidget shopButtonWidget;
        [SerializeField] private ButtonWidget backToFieldButton;
        [SerializeField] private ButtonWidget rollDiceButtonWidget;
        private int rollDiceCost = 100;
        [SerializeField] private Canvas canvas;

        private WalletDirector _walletDirector;
        private DiceDirector _diceDirector;
        private bool _isEnable = true;

        public override IEnumerator OnInitialize()
        {
            _diceDirector = DirectorFacade.GetDirector<DiceDirector>();
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            shopButtonWidget.onClickAction += ShowCanvas;
            backToFieldButton.onClickAction += HideCanvas;
            rollDiceButtonWidget.onClickAction += OnRollDiceButtonClick;
            yield return null;
        }

        public void OnRollDiceButtonClick()
        {
            if (_walletDirector.HasGold(rollDiceCost))
            {
                _walletDirector.SpendGold(rollDiceCost);
                _diceDirector.ShowCanvas();
                _diceDirector.ShowLayer();
                _diceDirector.Setup();
                HideCanvas();
            }
        }

        private void HideCanvas()
        {
            canvas.gameObject.SetActive(false);
        }

        private void ShowCanvas()
        {
            if (_isEnable)
            {
                canvas.gameObject.SetActive(true);
            }
        }

        public void SetEnable(bool value)
        {
            _isEnable = value;
        }
    }
}