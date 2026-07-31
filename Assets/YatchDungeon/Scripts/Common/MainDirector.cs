using System.Collections;
using KCoreKit;

namespace YatchDungeon
{
    public class MainDirector : DirectorBase
    {
        public override IEnumerator OnInitialize()
        {
            StartCoroutine(MainRoutine());
            yield return null;
        }

        private IEnumerator MainRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
            
        }
    }
}