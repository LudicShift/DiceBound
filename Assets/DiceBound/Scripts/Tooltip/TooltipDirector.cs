using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class TooltipDirector : DirectorBase
    {

        private Dictionary<string, TooltipWidget> _widgetDictionary;

        public void Awake()
        {
            _widgetDictionary =
                FindObjectsByType<TooltipWidget>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                    .ToDictionary(x => x.id);
        }

        public void BindTooltip(string id, TooltipProvider tooltipProvider, bool enabled = true)
        {
            tooltipProvider.BindWidget(_widgetDictionary[id],enabled);
        }
        
}
}