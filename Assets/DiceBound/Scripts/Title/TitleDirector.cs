using System.Collections;
using KCoreKit;
using UnityEngine.SceneManagement;

namespace DiceBound
{
    public class TitleDirector : DirectorBase
    {
        public ButtonWidget newGameButton;
        public ButtonWidget quitButton;

        public override IEnumerator OnInitialize()
        {
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
    }
}