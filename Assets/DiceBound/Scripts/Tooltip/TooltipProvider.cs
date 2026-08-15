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
        private bool _isHovered;

        public void SetText(string key, string text)
        {
            if (_context.textDictionary.ContainsKey(key))
            {
                _context.textDictionary[key] = text;
            }
            else
            {
                _context.textDictionary.TryAdd(key, text);
            }
        }

        public void Update()
        {
            if (_context is { enabled: true } && _isHovered)
            {
                _context.widget.OnUpdate(_context);
            }
        }


        public void BindWidget(TooltipWidget widget, bool enabled)
        {
            _context = new TooltipContext
            {
                widget = widget,
                enabled = enabled
            };
        }

        public void SetTooltipPosition(Vector3 position, Vector2 offset, bool screenSpace = true)
        {
            if (_context == null)
            {
                return;
            }

            _context.tooltipPosition = position;
            _context.offset = offset;
            _context.screenSpace = screenSpace;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_context is { enabled: true })
            {
                _context.widget.Show();
                _context.widget.OnShow(_context);
            }

            _isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_context is { enabled: true })
            {
                exitAction?.Invoke(_context);
                _context.widget.Hide();
            }

            _isHovered = false;
        }

        public void SetEnabled(bool value)
        {
            if (_context != null)
            {
                _context.enabled = value;
                if (!_context.enabled)
                {
                    _context.widget.Hide();
                }
            }
        }

        public bool IsHovered()
        {
            return _isHovered;
        }
    }
}