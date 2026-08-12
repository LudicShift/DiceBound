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
        [SerializeField]
        private ButtonWidget settingButton;
        [SerializeField]
        private SettingCanvas settingCanvas;

        public override IEnumerator OnInitialize()
        {
            mainDiscordButton.onClickAction+=OnMainDiscordButtonClick;

            newGameButton.onClickAction+=OnNewGameButtonClick;
            quitButton.onClickAction+=OnQuitButtonClick;

            if (settingButton)
            {
                settingButton.onClickAction+=OnSettingButtonClick;
            }

            yield return null;
        }

        private void OnSettingButtonClick()
        {
            if (settingCanvas)
            {
                settingCanvas.Show();
            }
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