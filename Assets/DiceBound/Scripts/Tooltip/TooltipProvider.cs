using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceBound
{
    public class TooltipProvider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TooltipContext _context;
        public readonly Action<TooltipContext> enterAction;
        public readonly Action<TooltipContext> exitAction;

        public void SetText(string text)
        {
            _context.text = text;
        }

        public void BindWidget(string id, TooltipWidget widget)
        {
            _context = new TooltipContext();
            _context.widget = widget;
        }

        public void SetTooltipPosition(string id, Vector2 position)
        {
            _context.tooltipPosition = position;
        }

        public void SetTooltipSize(string id, Vector2 size)
        {
            _context.tooltipSize = size;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_context.enabled)
            {
                enterAction?.Invoke(_context);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_context.enabled)
            {
                exitAction?.Invoke(_context);
            }
        }

        public void SetEnabled(string id, bool value)
        {
            _context.enabled = value;
        }

        public void SetTitleText(string id, string text)
        {
            _context.title = text;
        }
    }
}