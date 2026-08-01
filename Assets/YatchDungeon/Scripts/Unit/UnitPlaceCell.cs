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
        }

        public void OnHoverEnter()
        {
            _spriteRenderer.color = Color.red;
        }
        
        public void OnHoverExit()
        {
            _spriteRenderer.color = Color.white;
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