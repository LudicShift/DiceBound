using System.Collections;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class ShopDirector : DirectorBase
    {
        [BigHeader("General")]
        [SerializeField] private Canvas canvas;
        
        [BigHeader("Widget")]
        [SerializeField] private ButtonWidget shopButtonWidget;
        [SerializeField] private ButtonWidget backToFieldButton;
        [SerializeField] private ButtonWidget rollDiceButtonWidget;
        
        private int rollDiceCost = 100;
        private WalletDirector _walletDirector;
        private DiceDirector _diceDirector;
        private bool _isEnable = true;
        private UnitDirector _unitDirector;
        private UnitPlaceDirector _unitPlaceDirector;

        public override IEnumerator OnInitialize()
        {
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _diceDirector = DirectorFacade.GetDirector<DiceDirector>();
            _walletDirector = DirectorFacade.GetDirector<WalletDirector>();
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            shopButtonWidget.onClickAction += ShowCanvas;
            backToFieldButton.onClickAction += HideCanvas;
            rollDiceButtonWidget.onClickAction += OnRollDiceButtonClick;
            yield return null;
        }

        public void OnRollDiceButtonClick()
        {
            
            if (_walletDirector.HasGold(rollDiceCost) && !_unitDirector.IsAllyFull())
            {
                HideCanvas();
                _walletDirector.SpendGold(rollDiceCost);
                _diceDirector.ShowCanvas();
                _diceDirector.ShowLayer();
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
                canvas.gameObject.SetActive(true);
            }
        }

        public void SetEnable(bool value)
        {
            _isEnable = value;
        }
    }
}