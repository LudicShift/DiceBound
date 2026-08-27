using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using KCoreKit;
using NUnit.Framework;
using UnityEngine;

namespace DiceBound
{
    public class UnitFusionDirector : DirectorBase
    {
        [SerializeField] private UnitFusionPanelWidget panel;
        private List<UnitFusionDataTableRow> _unitFusionDataList;
        private UnitDirector _unitDirector;

        private List<UnitCore> _selectedTargetSources = new List<UnitCore>();

        public void OnBeginFusionState()
        {
            Refresh();
        }

        public void Refresh()
        {
            panel.OnUpdate(_unitDirector.GetAllies());
           
            foreach (var target in _selectedTargetSources)
            {
                target.SetHighlight(false);
            }

            _selectedTargetSources.Clear();
            foreach (var data in panel.GetSelectedFusionItem().GetInputUnitData())
            {
                if (data.target)
                {
                    data.target.SetHighlight(true,Color.chartreuse);
                    _selectedTargetSources.Add(data.target);
                }
            }
        }

        public void OnEndFusionState() //추후 리팩토링
        {
            foreach (var target in _selectedTargetSources)
            {
                target.SetHighlight(false);
            }

            _selectedTargetSources.Clear();
        }

        public override IEnumerator OnInitialize()
        {
            _unitDirector = DirectorFacade.GetDirector<UnitDirector>();
            _unitFusionDataList = DataTableManager.FindAllRows<UnitFusionDataTableRow>();
            var unitDictionary = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);
            panel.onSelectAction += OnSelectFusionItem;
            panel.Setup(_unitFusionDataList, unitDictionary,OnFusionButtonClick);
            yield return null;
        }

        private void OnFusionButtonClick()
        {
            var fusionItem = panel.GetSelectedFusionItem();
            var inputs = fusionItem.GetInputUnitData();
            if (inputs.Count(x => x.target == null) == 0)
            {
                Fusion(fusionItem);
            }
        }


        private void OnSelectFusionItem(FusionItemWidget widget)
        {
            Refresh();
        }

        private void Fusion(FusionItemWidget fusionItem)
        {
            StartCoroutine(FusionRoutine(fusionItem));
        }

        private IEnumerator FusionRoutine(FusionItemWidget fusionItem)
        {
            foreach (var source in _selectedTargetSources)
            {
                yield return source.ShowFusionEffect();
            }

            var resultUnit = _unitDirector.SpawnUnit(fusionItem.GetOutputUnitData().id, UnitGroup.Ally);
            yield return resultUnit.ShowFusionEffect();
            foreach (var source in _selectedTargetSources)
            {
                source.Move(resultUnit.transform.position);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var source in _selectedTargetSources)
            {
                yield return source.HideFusionEffect();
                _unitDirector.RemoveAllyUnit(source);
            }

            yield return resultUnit.HideFusionEffect();
            fusionItem.ClearSourceDataUnit();
            Refresh();
        }
    }
}