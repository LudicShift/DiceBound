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

        public void Awake()
        {
            RandomSystem.SetSeed((int)Time.time);
           
        }

        public override IEnumerator OnInitialize()
        {
            gameOverTitleButtonWidget.onClickAction+=OnTitleButtonClick;
            gameClearTitleButtonWidget.onClickAction+=OnTitleButtonClick;
             gameOverDiscordButton.onClickAction+=OnGameOverDiscordButtonClick;
            gameClearDiscordButton.onClickAction+=OnGameClearDiscordButtonClick;
            StartCoroutine(MainRoutine());
            AbilitySystem.Initialize();
            AbilitySystem.AddActionMethods(typeof(SkillAbilityAction));
            AbilitySystem.AddConditionMethods(typeof(SkillAbilityCondition));
            SkillAbilityAction.Setup();
            yield return null;
        }

        private IEnumerator MainRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
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