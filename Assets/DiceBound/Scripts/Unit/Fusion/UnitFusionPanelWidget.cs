using System;
using System.Collections.Generic;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitFusionPanelWidget : WidgetBase
    {
        [SerializeField] private RectTransform fusionItemRoot;
        [SerializeField] private ButtonWidget fusionButtonWidget;
        private List<FusionItemWidget> _fusionUnitWidgets = new List<FusionItemWidget>();

        private FusionDetailWidget _fusionDetailWidget;
        public Action<FusionItemWidget> onSelectAction;
        private FusionItemWidget _selectedFusionItem;

        public void Setup(List<UnitFusionDataTableRow> unitFusionDataList,
            Dictionary<string, UnitDataTableRow> unitDataDictionary,Action onFusionButtonClick)
        {
            
            fusionButtonWidget.onClickAction += onFusionButtonClick;
            _fusionDetailWidget = GetComponentInChildren<FusionDetailWidget>(true);
            _fusionDetailWidget.Setup();
            for (int i = 0; i < unitFusionDataList.Count; i++)
            {
                var fusionData = unitFusionDataList[i];
                var item = PrefabManager.Create<FusionItemWidget>();
                item.SetParent(fusionItemRoot);
                var inputList = new List<UnitDataTableRow>();
                foreach (var input in fusionData.GetInputs())
                {
                    inputList.Add(unitDataDictionary[input]);
                }

                item.Setup(unitDataDictionary[fusionData.outputUnit], inputList);
                _fusionUnitWidgets.Add(item);
                fusionItemRoot.sizeDelta += new Vector2(0, 103f);
            }

            foreach (var item in _fusionUnitWidgets)
            {
                item.onClickAction += () => OnSelectFusionItem(item);
            }

            OnSelectFusionItem(_fusionUnitWidgets[0]);
        }

        public void OnSelectFusionItem(FusionItemWidget item)
        {
            _selectedFusionItem = item;
            _fusionDetailWidget.OnChange(item);
            onSelectAction?.Invoke(item);
        }

        public void OnUpdate(List<UnitCore> unitList)
        {
            foreach (var widget in _fusionUnitWidgets)
            {
                widget.OnUpdate(unitList);
            }
            _fusionDetailWidget.OnChange(_selectedFusionItem);
        }

        public FusionItemWidget GetSelectedFusionItem()
        {
           return  _selectedFusionItem;
        }
    }
}