using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DG.Tweening;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitMergeDirector : DirectorBase
    {
        private UnitDirector _unitDirector;
        private ShopDirector _shopDirector;
        private SoundDirector _soundDirector;

        public override IEnumerator OnInitialize()
        {
            _soundDirector = DirectorFacade.GetDirector<SoundDirector>();
            _shopDirector = DirectorFacade.GetDirector<ShopDirector>();
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
            BroAudio.Play(_soundDirector.mergeUnitSFX);
            _shopDirector.SetEnable(false);
            foreach (var unit in unitList)
            {
                StartCoroutine(unit.ShowUpgradeEffect());
            }

            yield return new WaitForSeconds(0.1f);

            var mainUnit = unitList[0];
            unitList.Remove(mainUnit);

            int count = 0;
            for (int i = 0; i < unitList.Count; i++)
            {
                unitList[i].Move(mainUnit.transform.position).OnComplete(() =>
                {
                    mainUnit.transform.DOScale(0.2f, 0.05f).SetRelative().SetEase(Ease.OutBack);
                    count++;
                });
            }

            yield return new WaitUntil(() => count == unitList.Count);
            yield return new  WaitForSeconds(0.05f);

            foreach (var unit in unitList)
            {
                StartCoroutine(unit.HideUpgradeEffect());
                _unitDirector.RemoveAllyUnit(unit);
            }

            yield return mainUnit.transform.DOScale(1, 0.05f).WaitForCompletion();
            yield return mainUnit.HideUpgradeEffect();
            mainUnit.Upgrade();
            OnSpawnAlly(mainUnit);
            _shopDirector.SetEnable(true);
        }
    }
}