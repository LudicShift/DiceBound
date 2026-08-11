using System;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class UnitTrashCan : MonoBehaviour
    {
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
    }
}