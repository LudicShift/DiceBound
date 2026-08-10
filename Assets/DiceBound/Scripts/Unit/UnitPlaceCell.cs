using DG.Tweening;
using UnityEngine;

namespace DiceBound
{
    public class UnitPlaceCell : MonoBehaviour
    {
        private UnitCore _unit;
        public float distance;
        private SpriteRenderer _spriteRenderer;
        private bool _shown;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoverColor;
        
        public void Show()
        {
            _spriteRenderer.DOFade(0.5f, 0.2f);
            _shown = true;
        }
        
        
        public void Hide()
        {
            _spriteRenderer.DOFade(0.0f, 0.2f);
            _shown = false;
        }
        
        public void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void OnHoverEnter(bool value)
        {
            if (_shown && value)
            {
                _spriteRenderer.color = hoverColor;
            }
        }
        
        public void OnHoverExit()
        {
            if (_shown)
            {
                _spriteRenderer.color = normalColor;
            }
        }

        public bool IsEmpty()
        {
            return !_unit;
        }

        public void PlaceUnit(UnitCore unit, bool warp = true)
        {
            _unit = unit;
            _unit.SetParent(transform);
            if (warp)
            {
                _unit.Warp(Vector3.zero);
            }
            else
            {
                _unit.MoveTo(Vector3.zero);
            }
        }
        
        public void RemoveUnit()
        {
            _unit = null;
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