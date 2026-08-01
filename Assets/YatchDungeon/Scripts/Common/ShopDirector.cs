using System.Collections;
using KCoreKit;
using UnityEngine;

namespace YatchDungeon
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

        public override IEnumerator OnInitialize()
        {
            _diceDirector = DirectorFacade.GetSubMode<DiceDirector>();
            _walletDirector = DirectorFacade.GetSubMode<WalletDirector>();
            shopButtonWidget.AddOnClickAction(ShowCanvas);
            backToFieldButton.AddOnClickAction(HideCanvas);
            rollDiceButtonWidget.AddOnClickAction(OnRollDiceButtonClick);
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
            canvas.gameObject.SetActive(true);
        }
    }
}