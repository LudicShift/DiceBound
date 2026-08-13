using System;
using KCoreKit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceBound
{
    public class UnitTrashCan : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler
    {
        [SerializeField] private TweenAnimationPlayer clickPlayer;
        [SerializeField] private TweenAnimationPlayer hoverPlayer;
        [SerializeField] private TweenAnimationPlayer enterPlayer;
        [SerializeField] private TweenAnimationPlayer exitPlayer;
      
        
        
        public Action<UnitCore> onRemoveUnitAction;
        public void SetHighlight(bool value)
        {
            if (value)
            {
                StartCoroutine(enterPlayer.Play());
            }
            else
            {
                StartCoroutine(exitPlayer.Play());
            }
        }

        public void Execute(UnitCore unit)
        {
            onRemoveUnitAction?.Invoke(unit);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartCoroutine(hoverPlayer.Play());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(hoverPlayer.Play());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine(clickPlayer.Play());
        }
    }
}