
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

        [SerializeField] private ButtonWidget fusionButton;
        [SerializeField] private ButtonWidget upgradeButton;
        [SerializeField] private ButtonWidget shopButton;

        [SerializeField] private Canvas fusionCanvas;
        [SerializeField] private Canvas shopCanvas;
        [SerializeField] private Canvas upgradeCanvas;
        
        private PrepareState _prepareState = PrepareState.Shop;

        private ShopDirector _shopDirector;
        
        public override IEnumerator OnInitialize()
        {
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
            yield return null;
        }

        public void SetState(PrepareState prepareState)
        {
            _prepareState = prepareState;
            fusionButton.Show();
            upgradeButton.Show();
            shopButton.Show();
            shopCanvas.Close();
            shopCanvas.Close();
            shopCanvas.Close();
            
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
            
            
        }
    }
}