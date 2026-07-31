using System;
using System.Collections;
using KCoreKit;
using UnityEngine;

namespace YatchDungeon
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
            yield return null;
        }

        private IEnumerator MainRoutine()
        {
            yield return DirectorFacade.WaitUntilInitialized();
            
        }
    }
}