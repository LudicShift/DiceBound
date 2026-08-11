using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{

    public class TooltipWidget : WidgetBase
    {
        public string id;
        [SerializeField] private RectTransform root;
        [SerializeField] private TextWidget text;
        
        public void OnShow(TooltipContext context)
        {
            if (context.screenSpace)
            {
                root.anchoredPosition = new Vector2(context.tooltipPosition.x+context.offset.x,context.tooltipPosition.y+context.offset.y);
            }
            else
            {
                 SetPositionFromWorldPoint(CameraManager.GetMainCamera(),context.tooltipPosition,context.offset);
            }
            text.SetText(context.text);
        }

        public void OnUpdate(TooltipContext context)
        {
            if (context.screenSpace)
            {
                root.anchoredPosition = new Vector2(context.tooltipPosition.x+context.offset.x,context.tooltipPosition.y+context.offset.y);
            }
            else
            {
                SetPositionFromWorldPoint(CameraManager.GetMainCamera(),context.tooltipPosition,context.offset);
            }
            
        }
    }
}