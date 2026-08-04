using System.Collections;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class MainDirector : DirectorBase
    {
        public void Awake()
        {
            RandomSystem.SetSeed((int)Time.time);
           
        }

        public override IEnumerator OnInitialize()
        {
            StartCoroutine(MainRoutine());
            AbilitySystem.Initialize();
            AbilitySystem.AddActionMethods(typeof(AbilityAction));
            AbilitySystem.AddConditionMethods(typeof(AbilityCondition));
            AbilityAction.Setup();
            yield return null;
        }

        private IEnumerator MainRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
        }
    }
}