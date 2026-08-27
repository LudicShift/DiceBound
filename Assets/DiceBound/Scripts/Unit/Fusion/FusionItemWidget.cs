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

        public void Setup(UnitDataTableRow outputData, List<UnitDataTableRow> inputData)
        {
            _outputData = outputData;
            _sourceData = inputData.ConvertAll(x => new FusionSourceData(x));

            icon.sprite = _outputData.GetSprite();
            nameText.text = LocalizationManager.GetLocalizedText(_outputData.nameKey);
        }

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
            foreach (var source in _sourceData)
            {
                if (source.target == null)
                {
                    var target = unitList.Find(x => x.GetData() == source.unitData);
                    source.target = target;
                }
            }

            _percentage = 100f * _sourceData.Count(x => x.target != null) / (float)_sourceData.Count;
            percentageText.text = $"{_percentage:N0}%";
        }

        public float GetPercentage()
        {
            return _percentage;
        }
    }
}