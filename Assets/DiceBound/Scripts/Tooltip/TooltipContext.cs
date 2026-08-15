using System.Collections.Generic;
using UnityEngine;

namespace DiceBound
{
    public class TooltipContext
    {
        public TooltipWidget widget;
        public Vector3 tooltipPosition;
        public bool enabled;
        public bool screenSpace;
        public Vector2 offset;
        public Dictionary<string,string> textDictionary = new Dictionary<string, string>();
    }
}