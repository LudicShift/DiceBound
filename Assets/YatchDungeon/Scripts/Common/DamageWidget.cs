using System;
using System.Collections;
using DG.Tweening;
using KCoreKit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YatchDungeon
{
    public class DamageWidget : TextWidget
    {
        public override void Awake()
        {
          
        }

        public void Setup(int value)
        {
            SetText($"{value}");
        }

        public void Animate(Action<DamageWidget> callback)
        {
            Sequence seq = DOTween.Sequence();
            rectTransform.localScale = Vector3.zero;
            seq.Join(rectTransform.DOAnchorPos(new Vector2(Random.Range(-10,10)*10,100), 0.2f).SetRelative());
            seq.Join(transform.DOScale(1, 0.3f));
            seq.AppendInterval(0.3f);
            seq.Append(transform.DOScale(0, 0.3f));
            seq.AppendCallback(()=>callback.Invoke(this));
        }

      
    }
}