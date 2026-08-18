using DG.Tweening;
using UnityEngine;

namespace DiceBound
{
    public class UnitPlaceCell : UnitPlaceCellBase
    {
    
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

        public void Setup(float distance)
        {
            this.distance = distance;
        }

       
    }
}