using System;
using DG.Tweening;
using KCoreKit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DiceBound
{
    public class DamageWidget : TextWidget
    {
        private TweenAnimationSequenceConvertor _appear;

        public void Awake()
        {
            _appear = GetComponentInChildren<TweenAnimationSequenceConvertor>();
        }

        public void Setup(int value)
        {
            SetText($"{value}");
        }

        public void Play(Action<DamageWidget> callback)
        {
            Debug.Log("데미지");
            rectTransform.anchoredPosition += new Vector2(Random.Range(-150f, 150f), Random.Range(110f, 120f));
            //rectTransform.localScale = Vector3.zero;
            var seq = DOTween.Sequence();
            seq.JoinCallback(()=>_appear.Play());
            seq.AppendInterval(1f);
            seq.AppendCallback(() => { callback.Invoke(this); });
            seq.Play();
        }
    }
}