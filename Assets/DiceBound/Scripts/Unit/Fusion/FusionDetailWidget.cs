using System;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    public class FusionDetailWidget : WidgetBase
    {
        [SerializeField] private Image outputUnitIcon;
        [SerializeField] private TMP_Text  outputUnitNameText;
        [SerializeField] private TMP_Text  percentageText;

        private List<FusionSourceItemWidget> _sourceItemWidgets;

        public void Setup()
        {
            _sourceItemWidgets = GetComponentsInChildren<FusionSourceItemWidget>(true).ToList();
        }

        public void OnChange(FusionItemWidget selectedWidget)
        {
            var outputUnitData = selectedWidget.GetOutputUnitData();
            var sourceUnitData = selectedWidget.GetInputUnitData();
            // outputUnitData가 null이면 "동일 유닛 3개 -> 다음 등급 무작위 1체" 레시피라 결과를 미리 보여줄 수 없다.
            if (outputUnitData != null)
            {
                outputUnitIcon.sprite = outputUnitData.GetSprite();
                outputUnitNameText.text = LocalizationManager.GetLocalizedText(outputUnitData.nameKey);
            }
            else
            {
                outputUnitIcon.sprite = null;
                outputUnitNameText.text = "???";
            }
            percentageText.text = $"{selectedWidget.GetPercentage():N0}%";
            ClearAllSource();
            for (int i = 0; i < sourceUnitData.Count; i++)
            {
                _sourceItemWidgets[i].Setup(sourceUnitData[i]);
                _sourceItemWidgets[i].Show();
                if (sourceUnitData[i].target)
                {
                    _sourceItemWidgets[i].Mark();
                }
            }
        }

        private void ClearAllSource()
        {
            foreach (var item in _sourceItemWidgets)
            {
                item.Clear();
                item.Hide();
            }
        }
    }
}