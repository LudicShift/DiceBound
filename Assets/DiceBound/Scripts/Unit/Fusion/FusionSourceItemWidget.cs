using KCoreKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceBound
{
    public class FusionSourceItemWidget : WidgetBase
    {
        [SerializeField] private Image unitIconImage;
        [SerializeField] private TMP_Text unitNameText;
        [SerializeField] private Image checkBox;
        
        private FusionSourceData _data;
        [SerializeField] private Sprite unMarkSprite;
        [SerializeField] private Sprite markSprite;


        public void Setup(FusionSourceData data)
        {
            unitIconImage.sprite = data.unitData.GetSprite();
            unitNameText.text = LocalizationManager.GetLocalizedText(data.unitData.nameKey);
            _data = data;
        }

        public FusionSourceData GetData()
        {
            return _data;
        }

        public void Clear()
        {
            _data = null;
            checkBox.sprite = unMarkSprite;
        }

        public void Mark()
        {
            checkBox.sprite = markSprite;
        }
    }
}