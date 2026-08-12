using System.Collections;
using KCoreKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceBound
{
    public class TitleDirector : DirectorBase
    {
        public ButtonWidget newGameButton;
        public ButtonWidget quitButton;
        [SerializeField] 
        private ButtonWidget mainDiscordButton;

        public override IEnumerator OnInitialize()
        {
            mainDiscordButton.onClickAction+=OnMainDiscordButtonClick;

            newGameButton.onClickAction+=OnNewGameButtonClick;
            quitButton.onClickAction+=OnQuitButtonClick;
            yield return null;
        }

        private void OnQuitButtonClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnNewGameButtonClick()
        {
            SceneManager.LoadScene("BattleScene");
        }
        private void OnMainDiscordButtonClick()
        {
            Application.OpenURL("https://discord.gg/KMwkWV8wW7");
        }     
    }
}