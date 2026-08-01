using System;
using UnityEngine;

namespace YatchDungeon
{
    public class UnitPlaceCell : MonoBehaviour
    {
        private UnitCore _unit;
        public float distance;
        private SpriteRenderer _spriteRenderer;

        public void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _spriteRenderer.color = new Color32  (6, 169, 250, 192);
        }

        public void OnHoverEnter(bool value)
        {
            if (value)
            {
                _spriteRenderer.color = new Color32 (10, 250, 6, 192);
            }
        }
        
        public void OnHoverExit()
        {
            _spriteRenderer.color = new Color32  (6, 169, 250, 192);
        }

        public bool IsEmpty()
        {
            return !_unit;
        }

        public void PushUnit(UnitCore unit)
        {
            _unit = unit;
        }
        
        public UnitCore PopUnit()
        {
            var unit = _unit;
            _unit = null;
            return unit;
        }

        public void Setup(float distance)
        {
            this.distance = distance;
        }

        public UnitCore GetUnit()
        {
            return _unit;
        }
    }
}