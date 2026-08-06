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
        private TweenAnimationCombiner tween;
        
        public void Setup(int value)
        {
            SetText($"{value}");
        }

        public void Play(Action<DamageWidget> callback)
        {
            //rectTransform.anchoredPosition += new Vector2(Random.Range(-150f, 150f), Random.Range(110f, 120f));
            StartCoroutine(PlayAnimation(callback));
        }

        private IEnumerator PlayAnimation(Action<DamageWidget> callback)
        {
            yield return tween.Play();
            callback?.Invoke(this);
        }
    }
}