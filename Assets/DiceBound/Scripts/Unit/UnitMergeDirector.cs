using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitMergeDirector : DirectorBase
    {
        private UnitDirector _unitDirector;

        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitDirector.onSpawnAlly += OnSpawnAlly;
            yield return null;
        }

        public void OnSpawnAlly(UnitCore unit)
        {
            if (unit.GetTier() == 2)
            {
                return;
            }

            var allies = _unitDirector.GetAllies();
            var mergeTarget = new List<UnitCore>();
            foreach (var ally in allies)
            {
                if (unit.GetId() == ally.GetId() && unit.GetTier() == ally.GetTier())
                {
                    mergeTarget.Add(ally);
                }
            }

            if (mergeTarget.Count >= 3)
            {
                StartCoroutine(MergeUnit(mergeTarget));
            }
        }

        private IEnumerator MergeUnit(List<UnitCore> unitList)
        {
            foreach (var unit in unitList)
            {
                StartCoroutine(unit.ShowUpgradeEffect());
            }

            yield return new WaitForSeconds(0.2f);

            var mainUnit = unitList[0];
            unitList.Remove(mainUnit);

            int count = 0;
            for (int i = 0; i < unitList.Count; i++)
            {
                unitList[i].Move(mainUnit.transform.position).SetDelay(i * 0.3f).OnComplete(() =>
                {
                    mainUnit.transform.DOScale(0.2f, 0.1f).SetRelative().SetEase(Ease.OutElastic);
                    count++;
                });
            }

            yield return new WaitUntil(() => count == unitList.Count);

            foreach (var unit in unitList)
            {
                StartCoroutine(unit.HideUpgradeEffect());
                _unitDirector.RemoveAllyUnit(unit);
            }

            yield return mainUnit.transform.DOScale(1, 0.3f).WaitForCompletion();
            yield return mainUnit.HideUpgradeEffect();
            mainUnit.Upgrade();
            OnSpawnAlly(mainUnit);
        }
    }
}