
using System;
using System.Collections;
using DiceBound.Shop;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public enum PrepareState
    {
        Shop,
        Fusion,
        Upgrade,
    }
    
    public class PreparePhaseDirectorBase : PhaseDirectorBase
    {
        [SerializeField]
        private Canvas prepareCanvas;

        [SerializeField] private ButtonWidget battleButton;
        
        [SerializeField] private ButtonWidget fusionButton;
        [SerializeField] private ButtonWidget upgradeButton;
        [SerializeField] private ButtonWidget shopButton;

        [SerializeField] private Canvas fusionCanvas;
        [SerializeField] private Canvas shopCanvas;
        [SerializeField] private Canvas upgradeCanvas;
        
        private PrepareState _prepareState = PrepareState.Shop;

        private ShopDirector _shopDirector;
        private GameDirector _gameDirector;

        public override IEnumerator OnInitialize()
        {
            battleButton.onClickAction += OnBattleButtonCLick;
            fusionButton.onClickAction += OnFusionButtonClick;
            upgradeButton.onClickAction += OnUpgradeButtonClick;
            shopButton.onClickAction += OnShopButtonClick;
            _gameDirector = DirectorFacade.GetDirector<GameDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
            yield return null;
        }

        private void OnShopButtonClick()
        {
            SetState(PrepareState.Shop);
        }

        private void OnUpgradeButtonClick()
        {
            SetState(PrepareState.Upgrade);
        }

        private void OnFusionButtonClick()
        {
            SetState(PrepareState.Fusion);
        }

        private void OnBattleButtonCLick()
        {
            _gameDirector.SetGamePhase(GamePhase.Battle);
        }

        public void SetState(PrepareState prepareState)
        {
            _prepareState = prepareState;
            fusionButton.Show();
            upgradeButton.Show();
            shopButton.Show();
            shopCanvas.Close();
            fusionCanvas.Close();
            upgradeCanvas.Close();
            
            switch (_prepareState)
            {
                case PrepareState.Shop:
                    shopButton.Hide();
                    shopCanvas.Open();
                    break;
                case PrepareState.Fusion:
                    fusionButton.Hide();
                    fusionCanvas.Open();
                    break;
                case PrepareState.Upgrade:
                    upgradeButton.Hide();
                    upgradeCanvas.Open();
                    break;
            }
        }
        public override void OnEnter()
        {
            prepareCanvas.Open();
            _shopDirector.Refresh();
            SetState(PrepareState.Shop); //항상 상점 먼저 열림
        }

        public override void OnExit()
        {
            prepareCanvas.Close();
            shopCanvas.Close();
            fusionCanvas.Close();
            upgradeCanvas.Close();
        }
    }
}