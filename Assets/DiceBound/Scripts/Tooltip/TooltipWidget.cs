using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public enum PositionMode
    {
        Normal,
        Anchored
    }
    public class TooltipWidget : WidgetBase
    {
        public string id;
        [SerializeField] private RectTransform root;
        [SerializeField] private TextWidget title;
        [SerializeField] private TextWidget text;
        public PositionMode positionMode;
        public bool isEnable;

        public void SetEnabled(bool value)
        {
            isEnable = value;
        }

        public void OnShow(TooltipContext context)
        {
            switch (positionMode)
            {
                case PositionMode.Normal:
                    root.position = context.tooltipPosition;
                    break;
                case PositionMode.Anchored:
                    root.anchoredPosition = context.tooltipPosition;
                    break;
            }
      
            root.sizeDelta = context.tooltipSize;
            text.SetFont(LocalizationManager.GetFontAsset(0));
            text.SetText(context.text);
            title?.SetFont(LocalizationManager.GetFontAsset(0));
            title?.SetText(context.title);
        }
    }
}