using System;
using System.Collections;
using System.Collections.Generic;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public enum GamePhase
    {
        None,
        Prepare,
        Battle,
        GameOver,
        GameClear
    }

    public class GameDirector : DirectorBase
    {
        [SerializeField] private ButtonWidget gameOverTitleButtonWidget;
        [SerializeField] private ButtonWidget gameClearTitleButtonWidget;

        [SerializeField] private ButtonWidget gameOverDiscordButton;
        [SerializeField] private ButtonWidget gameClearDiscordButton;

        private GamePhase _gamePhase;

        private Dictionary<GamePhase, PhaseDirectorBase> _phaseDirectorDictionary =
            new Dictionary<GamePhase, PhaseDirectorBase>();

        public void Awake()
        {
            RandomSystem.SetSeed((int)Time.time);
        }

        public override IEnumerator OnInitialize()
        {
            StartCoroutine(GameRoutine());
            _phaseDirectorDictionary.TryAdd(GamePhase.Prepare, DirectorFacade.GetDirector<PreparePhaseDirectorBase>());
            _phaseDirectorDictionary.TryAdd(GamePhase.Battle, DirectorFacade.GetDirector<BattlePhaseDirectorBase>());
            _phaseDirectorDictionary.TryAdd(GamePhase.GameOver, null);
            _phaseDirectorDictionary.TryAdd(GamePhase.GameClear,null);
            SetGamePhase(GamePhase.Prepare);

            gameOverTitleButtonWidget.onClickAction += OnTitleButtonClick;
            gameClearTitleButtonWidget.onClickAction += OnTitleButtonClick;
            gameOverDiscordButton.onClickAction += OnGameOverDiscordButtonClick;
            gameClearDiscordButton.onClickAction += OnGameClearDiscordButtonClick;
            AbilitySystem.Initialize();
            AbilitySystem.AddActionMethods(typeof(SkillAbilityAction));
            AbilitySystem.AddConditionMethods(typeof(SkillAbilityCondition));
            SkillAbilityAction.Setup();
            yield return null;
        }

        private IEnumerator GameRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
        }

        public void SetGamePhase(GamePhase gamePhase)
        {
            if (_gamePhase != GamePhase.None)
            {
                _phaseDirectorDictionary[_gamePhase]?.OnExit();
            }
            
            _gamePhase = gamePhase;
            
            if (_gamePhase != GamePhase.None)
            {
                _phaseDirectorDictionary[_gamePhase]?.OnEnter();
            }
        }

        private void OnTitleButtonClick()
        {
            LoadingCanvas.FadeOut(() => { LoadingManager.LoadScene("TitleScene", () => LoadingCanvas.FadeIn()); });
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