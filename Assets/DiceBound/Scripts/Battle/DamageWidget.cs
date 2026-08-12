using System;
using System.Collections;
using DG.Tweening;
using KCoreKit;
using UnityEngine;

namespace DiceBound
{
    public class DamageWidget : TextWidget
    {
        [SerializeField]
        private TweenAnimationPlayer tween;
        
        public void Setup(int value)
        {
            if (value == 0)
            {
                SetText($"Miss");
            }
            else
            {
                SetText($"{value}");
            }
        }

        public void Play(Action<DamageWidget> callback)
        {
            //rectTransform.anchoredPosition += new Vector2(Random.Range(-150f, 150f), Random.Range(110f, 120f));
            StartCoroutine(PlayAnimation(callback));
        }

        private IEnumerator PlayAnimation(Action<DamageWidget> callback)
        {
            yield return tween.Play();
            //yield return new WaitForSeconds(0.5f);
            callback?.Invoke(this);
        }

    
    }
}