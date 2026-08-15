using System;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{

    public class TooltipWidget : WidgetBase
    {
        public string id;
        [SerializeField] private RectTransform root;
        private Dictionary<string,TooltipTextWidget> textDictionary;

        public void Awake()
        {
            textDictionary = GetComponentsInChildren<TooltipTextWidget>().ToDictionary(x => x.key);
        }

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

            foreach (var text in context.textDictionary)
            {
                textDictionary[text.Key].SetText(text.Value);
            }
           
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