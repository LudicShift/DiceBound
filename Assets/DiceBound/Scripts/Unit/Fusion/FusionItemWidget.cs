using System;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    public class FusionSourceData
    {
        public UnitDataTableRow unitData;
        public UnitCore target;

        public FusionSourceData(UnitDataTableRow input)
        {
            unitData = input;
        }
    }

    public class FusionItemWidget : ButtonWidget
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text percentageText;

        private UnitDataTableRow _outputData;
        private List<FusionSourceData> _sourceData;
        private float _percentage;
        private bool _isRandomGradeUp;
        private bool _isWildcard;
        private string _wildcardGrade;
        private int _requiredCount;

        // outputData가 null이면 "동일 유닛 3개 -> 다음 등급 무작위 1체" 레시피 - 결과를 미리 보여줄 수 없음
        public void Setup(UnitDataTableRow outputData, List<UnitDataTableRow> inputData, bool isRandomGradeUp = false)
        {
            _outputData = outputData;
            _isRandomGradeUp = isRandomGradeUp;
            _isWildcard = false;
            _wildcardGrade = null;
            _sourceData = inputData.ConvertAll(x => new FusionSourceData(x));

            if (_outputData != null)
            {
                icon.sprite = _outputData.GetSprite();
                nameText.text = LocalizationManager.GetLocalizedText(_outputData.nameKey);
            }
            else
            {
                icon.sprite = null;
                nameText.text = "???";
            }
        }

        // "이 등급에 속하는 아무 유닛이나 requiredCount개 보유 -> 다음 등급 무작위 1체" - 특정 유닛에 묶이지 않는다.
        // 실제 슬롯은 OnUpdate에서 그 시점 보유 로스터를 보고 매번 다시 찾는다.
        public void SetupWildcard(string wildcardGrade, int requiredCount, string displayName)
        {
            _outputData = null;
            _isRandomGradeUp = true;
            _isWildcard = true;
            _wildcardGrade = wildcardGrade;
            _requiredCount = requiredCount;
            _sourceData = new List<FusionSourceData>();

            icon.sprite = null;
            nameText.text = displayName;
        }

        public bool IsRandomGradeUp => _isRandomGradeUp;

        public UnitDataTableRow GetOutputUnitData()
        {
            return _outputData;
        }

        public List<FusionSourceData> GetInputUnitData()
        {
            return _sourceData;
        }

        public void ClearSourceDataUnit()
        {
            foreach (var data in _sourceData)
            {
                data.target = null;
            }
        }

        public void OnUpdate(List<UnitCore> unitList)
        {
            if (_isWildcard)
            {
                OnUpdateWildcard(unitList);
                return;
            }

            // 같은 유닛 타입을 요구하는 슬롯이 여러 개(예: 동일 유닛 3개 승급)일 때,
            // 전부 같은 물리 인스턴스 하나에 몰리지 않도록 이미 배정된 인스턴스는 제외하고 찾는다.
            var alreadyAssigned = new HashSet<UnitCore>();
            foreach (var source in _sourceData)
            {
                if (source.target != null)
                {
                    alreadyAssigned.Add(source.target);
                }
            }

            foreach (var source in _sourceData)
            {
                if (source.target == null)
                {
                    var target = unitList.Find(x => x.GetData() == source.unitData && !alreadyAssigned.Contains(x));
                    source.target = target;
                    if (target != null)
                    {
                        alreadyAssigned.Add(target);
                    }
                }
            }

            _percentage = 100f * _sourceData.Count(x => x.target != null) / (float)_sourceData.Count;
            percentageText.text = $"{_percentage:N0}%";
        }

        public float GetPercentage()
        {
            return _percentage;
        }

        // 보유 로스터 중 이 등급의 유닛을 종류별로 세어, requiredCount개 이상 모인 첫 종류를 이번 슬롯에 배정한다.
        // 플레이어가 팔거나 새로 얻는 등 로스터가 바뀔 수 있어 매번 다시 계산한다.
        private void OnUpdateWildcard(List<UnitCore> unitList)
        {
            var grouped = unitList
                .Where(x => x.GetData().grade == _wildcardGrade)
                .GroupBy(x => x.GetData().id)
                .FirstOrDefault(g => g.Count() >= _requiredCount);

            _sourceData = grouped != null
                ? grouped.Take(_requiredCount).Select(x => new FusionSourceData(x.GetData()) { target = x }).ToList()
                : new List<FusionSourceData>();

            _percentage = _sourceData.Count > 0 ? 100f : 0f;
            percentageText.text = $"{_percentage:N0}%";
        }
    }
}