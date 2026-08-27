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
            outputUnitIcon.sprite = outputUnitData.GetSprite();
            outputUnitNameText.text = LocalizationManager.GetLocalizedText(outputUnitData.nameKey);
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