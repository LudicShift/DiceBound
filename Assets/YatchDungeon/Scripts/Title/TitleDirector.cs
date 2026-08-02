using System.Collections;
using KCoreKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YatchDungeon.Title
{
    public class TitleDirector : DirectorBase
    {
        public ButtonWidget newGameButton;
        public ButtonWidget quitButton;

        public override IEnumerator OnInitialize()
        {
            newGameButton.AddOnClickAction(OnNewGameButtonClick);
            quitButton.AddOnClickAction(OnQuitButtonClick);
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
    }
}