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
        private UnitPlaceDirector _unitPlaceDirector;
        private Dictionary<string, UnitDataTableRow> _unitDictionary;

        // 등급 승급 순서 (레전더리는 이 자동 승급 대상이 아니고, DT_Fusion.csv의 전용 레시피로만 도달)
        private static readonly Dictionary<string, string> NextGrade = new Dictionary<string, string>
        {
            { "Common", "Rare" },
            { "Rare", "Epic" },
        };

        private List<UnitCore> _selectedTargetSources = new List<UnitCore>();
        private bool _isFusing; // 합성 연출 도중 중복 클릭 방지 (안 그러면 같은 재료가 두 번 제거되면서 pool Release 예외 발생)

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

            panel.SetFusionButtonInteractable(CanFuse(panel.GetSelectedFusionItem()));
        }

        // 재료 슬롯이 하나 이상 있고, 전부 채워져야 합성 가능. 슬롯이 0개인 경우(와일드카드 재료 미보유)는 채워진 게 아니라 "불가능"이다.
        private bool CanFuse(FusionItemWidget fusionItem)
        {
            var inputs = fusionItem.GetInputUnitData();
            return inputs.Count > 0 && inputs.All(x => x.target != null);
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
            _unitPlaceDirector = DirectorFacade.GetDirector<UnitPlaceDirector>();
            _unitFusionDataList = DataTableManager.FindAllRows<UnitFusionDataTableRow>();
            _unitDictionary = DataTableManager.FindAllRows<UnitDataTableRow>().ToDictionary(x => x.id);
            panel.onSelectAction += OnSelectFusionItem;
            panel.Setup(_unitFusionDataList, _unitDictionary,OnFusionButtonClick);
            yield return null;
        }

        private void OnFusionButtonClick()
        {
            var fusionItem = panel.GetSelectedFusionItem();
            if (CanFuse(fusionItem))
            {
                Fusion(fusionItem);
            }
        }


        private void OnSelectFusionItem(FusionItemWidget widget)
        {
            Refresh();
        }

        // "동일 유닛 N개 -> 다음 등급 무작위 1체" 레시피의 실제 결과를 지금 이 순간에 뽑는다 (미리 정해두지 않음).
        // 재료와 같은 종족끼리만 승급한다 (종족 정체성 유지). Angel은 이 무작위 승급 대상이 아니고 DT_Fusion.csv의
        // 전용 고정 레시피(FR_ANGEL_PRIEST/FR_ANGEL_KNIGHT)로만 도달한다 - 애초에 Angel 유닛은 재료로 쓰일 일이 없다.
        private string ResolveRandomGradeUpOutput()
        {
            var source = _selectedTargetSources[0].GetData();
            if (!NextGrade.TryGetValue(source.grade, out var targetGrade))
            {
                Debug.LogError($"[UnitFusionDirector] '{source.grade}' 등급은 무작위 승급 대상이 아닙니다.");
                return source.id;
            }

            var candidates = _unitDictionary.Values.Where(x => x.grade == targetGrade && x.race == source.race).ToList();
            if (candidates.Count == 0)
            {
                Debug.LogError($"[UnitFusionDirector] '{source.race}' 종족의 '{targetGrade}' 등급 유닛이 없습니다.");
                return source.id;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)].id;
        }

        private void Fusion(FusionItemWidget fusionItem)
        {
            if (_isFusing)
            {
                return;
            }

            _isFusing = true;
            StartCoroutine(FusionRoutine(fusionItem));
        }

        private IEnumerator FusionRoutine(FusionItemWidget fusionItem)
        {
            // 연출 도중 로스터가 갱신(Refresh)돼 _selectedTargetSources가 바뀌어도 흔들리지 않도록 스냅샷을 뜬다.
            var sources = new List<UnitCore>(_selectedTargetSources);

            // 재료가 여러 개일 때 순서대로(foreach + yield) 하나씩 기다리면 재료 수만큼 시간이 배로 늘어난다.
            // 서로 독립적인 연출이므로 동시에 재생하고 전부 끝날 때만 기다린다.
            yield return PlayAllTogether(sources.Select(x => x.ShowFusionEffect()));

            // 재료를 배치판에서 먼저 비워야 한다 (게임오브젝트/로스터는 아직 유지 - 날아가는 연출용).
            // 안 그러면 로스터가 꽉 찼을 때 결과 유닛이 배치될 빈 칸이 없어 SpawnUnit이 크래시한다.
            foreach (var source in sources)
            {
                _unitPlaceDirector.RemoveUnit(source);
            }

            var outputId = fusionItem.IsRandomGradeUp
                ? ResolveRandomGradeUpOutput()
                : fusionItem.GetOutputUnitData().id;
            var resultUnit = _unitDirector.SpawnUnit(outputId, UnitGroup.Ally);
            yield return resultUnit.ShowFusionEffect();
            foreach (var source in sources)
            {
                source.Move(resultUnit.transform.position);
            }

            yield return new WaitForSeconds(0.3f);

            // 재료들 + 결과 유닛의 페이드아웃도 전부 동시에 재생한다.
            yield return PlayAllTogether(sources.Select(x => x.HideFusionEffect()).Append(resultUnit.HideFusionEffect()));

            foreach (var source in sources)
            {
                _unitDirector.RemoveAllyUnit(source);
            }

            fusionItem.ClearSourceDataUnit();
            _isFusing = false;
            Refresh();
        }

        // 서로 독립적인 IEnumerator 연출들을 동시에 재생하고, 전부 끝날 때까지(가장 오래 걸리는 것 기준) 기다린다.
        private IEnumerator PlayAllTogether(IEnumerable<IEnumerator> routines)
        {
            var coroutines = routines.Select(StartCoroutine).ToList();
            foreach (var co in coroutines)
            {
                yield return co;
            }
        }
    }
}