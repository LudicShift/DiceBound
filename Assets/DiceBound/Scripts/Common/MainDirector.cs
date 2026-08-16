using System.Collections;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MainDirector : DirectorBase
    {
        [SerializeField] private ButtonWidget gameOverTitleButtonWidget;
        [SerializeField] private ButtonWidget gameClearTitleButtonWidget;
  
       [SerializeField] private ButtonWidget gameOverDiscordButton;
        [SerializeField] private ButtonWidget gameClearDiscordButton;
        private ShopDirector _shopDirector;

        public void Awake()
        {
            RandomSystem.SetSeed((int)Time.time);
           
        }

        public override IEnumerator OnInitialize()
        {
            StartCoroutine(MainRoutine());
            gameOverTitleButtonWidget.onClickAction+=OnTitleButtonClick;
            gameClearTitleButtonWidget.onClickAction+=OnTitleButtonClick;
             gameOverDiscordButton.onClickAction+=OnGameOverDiscordButtonClick;
            gameClearDiscordButton.onClickAction+=OnGameClearDiscordButtonClick;
            AbilitySystem.Initialize();
            AbilitySystem.AddActionMethods(typeof(SkillAbilityAction));
            AbilitySystem.AddConditionMethods(typeof(SkillAbilityCondition));
            SkillAbilityAction.Setup();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
            yield return null;
        }

        private IEnumerator MainRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
            _shopDirector.OnRoundBegin();
        }
        
        private void OnTitleButtonClick()
        {
            LoadingCanvas.FadeOut(() =>
            {
                LoadingManager.LoadScene("TitleScene",()=>LoadingCanvas.FadeIn());
            });
        }
        
        private void OnGameOverDiscordButtonClick()
        {
            Application.OpenURL("https://discord.gg/GFYnRbPYeJ");
        }
        
        private void OnGameClearDiscordButtonClick()
        {
            Application.OpenURL("https://discord.gg/BT3U3ZUYXK");
        }
    }
}